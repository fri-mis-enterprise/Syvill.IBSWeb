using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Models.ViewModels;
using IBS.Services.Attributes;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
    public class DebitMemoController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DebitMemoController> _logger;

        public DebitMemoController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<DebitMemoController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private async Task<string?> GetCompanyClaimAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == "Company")?.Value;
        }

        public IActionResult Index(string? view)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetDebitMemos([FromForm] DataTablesParameters parameters, DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                var debitMemos = _unitOfWork.DebitMemo
                    .GetAllQuery();

                var totalRecords = await debitMemos.CountAsync(cancellationToken);

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasTransactionDate = DateOnly.TryParse(searchValue, out var transactionDate);

                    debitMemos = debitMemos
                        .Where(s =>
                            s.DebitMemoNo!.ToLower().Contains(searchValue) ||
                            s.ServiceInvoice!.ServiceInvoiceNo.ToLower().Contains(searchValue) == true ||
                            (hasTransactionDate && s.TransactionDate == transactionDate) ||
                            s.DebitAmount.ToString().Contains(searchValue) ||
                            s.Remarks!.ToLower().Contains(searchValue) == true ||
                            s.Description.ToLower().Contains(searchValue) ||
                            s.CreatedBy!.ToLower().Contains(searchValue)
                            );
                }
                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    debitMemos = debitMemos.Where(s => s.TransactionDate == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    debitMemos = debitMemos
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await debitMemos.CountAsync(cancellationToken);

                var pagedData = await debitMemos
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
                    .ToListAsync(cancellationToken);

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalFilteredRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get debit memo. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var viewModel = new DebitMemoViewModel();
            await IncludeSelectLists(viewModel, cancellationToken);
            return View(viewModel);
        }

        public async Task IncludeSelectLists(DebitMemoViewModel viewModel, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();


            viewModel.ServiceInvoices = (await _unitOfWork.ServiceInvoice
                .GetAllAsync(sv => sv.PostedBy != null, cancellationToken))
                .Select(sv => new SelectListItem
                {
                    Value = sv.ServiceInvoiceId.ToString(),
                    Text = sv.ServiceInvoiceNo
                })
                .ToList();

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.DebitMemo, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DebitMemoViewModel viewModel, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(viewModel);
            }

            var model = new DebitMemo
            {
                Source = viewModel.Source,
                TransactionDate = viewModel.TransactionDate,
                ServiceInvoiceId = viewModel.ServiceInvoiceId,
                Period = viewModel.Period,
                Amount = viewModel.Amount,
                Remarks = viewModel.Remarks,
                Description = viewModel.Description,
            };

            var existingSv = await _unitOfWork.ServiceInvoice
                        .GetAsync(sv => sv.ServiceInvoiceId == model.ServiceInvoiceId, cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region -- checking for unposted DM or CM

                var existingSVDMs = (await _unitOfWork.DebitMemo
                        .GetAllAsync(si => si.ServiceInvoiceId == model.ServiceInvoiceId && si.PostedBy != null && si.CanceledBy != null && si.VoidedBy != null, cancellationToken))
                    .OrderBy(s => s.DebitMemoId)
                    .ToList();
                if (existingSVDMs.Count > 0)
                {
                    await IncludeSelectLists(viewModel, cancellationToken);
                    ModelState.AddModelError("", $"Can’t proceed to create you have unposted DM/CM. {existingSVDMs.First().DebitMemoNo}");
                    return View(viewModel);
                }

                var existingSVCMs = (await _unitOfWork.CreditMemo
                        .GetAllAsync(si => si.ServiceInvoiceId == model.ServiceInvoiceId && si.PostedBy != null && si.CanceledBy != null && si.VoidedBy != null, cancellationToken))
                    .OrderBy(s => s.CreditMemoId)
                    .ToList();
                if (existingSVCMs.Count > 0)
                {
                    await IncludeSelectLists(viewModel, cancellationToken);
                    ModelState.AddModelError("", $"Can’t proceed to create you have unposted DM/CM. {existingSVCMs.First().CreditMemoNo}");
                    return View(viewModel);
                }

                #endregion -- checking for unposted DM or CM

                model.CreatedBy = GetUserFullName();

                model.DebitMemoNo = await _unitOfWork.DebitMemo.GenerateCodeAsync(companyClaims, existingSv!.Type, cancellationToken);
                model.Type = existingSv.Type;
                model.DebitAmount = model.Amount;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!, $"Create new debit memo# {model.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.DebitMemo.AddAsync(model, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Debit memo #{model.DebitMemoNo} created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                _logger.LogError(ex, "Failed to create debit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Print(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitMemo = await _unitOfWork.DebitMemo.GetAsync(dm => dm.DebitMemoId == id, cancellationToken);
            if (debitMemo == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview debit memo# {debitMemo.DebitMemoNo}", "Debit Memo");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(debitMemo);
        }

        public async Task<IActionResult> Post(int id, ViewModelDMCM viewModelDmcm, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.DebitMemo.GetAsync(dm => dm.DebitMemoId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.IsPeriodPostedAsync(Module.DebitMemo, model.TransactionDate, cancellationToken))
                {
                    throw new ArgumentException($"Cannot post this record because the period {model.TransactionDate:MMM yyyy} is already closed.");
                }

                model.PostedBy = GetUserFullName();
                model.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Posted);

                var accountTitlesDto = await _unitOfWork.ServiceInvoice.GetListOfAccountTitleDto(cancellationToken);
                var arTradeReceivableTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020100") ?? throw new ArgumentException("Account title '101020100' not found.");
                var arNonTradeTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020500") ?? throw new ArgumentException("Account title '101020500' not found.");
                var arTradeCwt = accountTitlesDto.Find(c => c.AccountNumber == "101020200") ?? throw new ArgumentException("Account title '101020200' not found.");
                var arTradeCwv = accountTitlesDto.Find(c => c.AccountNumber == "101020300") ?? throw new ArgumentException("Account title '101020300' not found.");
                var vatOutputTitle = accountTitlesDto.Find(c => c.AccountNumber == "201030100") ?? throw new ArgumentException("Account title '201030100' not found.");

                var existingSv = await _unitOfWork.ServiceInvoice
                        .GetAsync(sv => sv.ServiceInvoiceId == model.ServiceInvoiceId, cancellationToken);

                #region -- Computation --

                viewModelDmcm.Period = DateOnly.FromDateTime(model.CreatedDate) >= model.Period ? DateOnly.FromDateTime(model.CreatedDate) : model.Period.AddMonths(1).AddDays(-1);

                var netDiscount = model.Amount - existingSv!.Discount;
                var netOfVatAmount = model.ServiceInvoice!.VatType == SD.VatType_Vatable
                    ? _unitOfWork.ServiceInvoice.ComputeNetOfVat(netDiscount)
                    : netDiscount;
                var vatAmount = model.ServiceInvoice!.VatType == SD.VatType_Vatable
                    ? _unitOfWork.ServiceInvoice.ComputeVatAmount(netOfVatAmount)
                    : 0m;
                var ewt = model.ServiceInvoice!.HasEwt
                    ? _unitOfWork.DebitMemo.ComputeEwtAmount(netOfVatAmount, existingSv!.ServicePercent / 100m)
                    : 0m;

                var wvat = model.ServiceInvoice!.HasWvat
                    ? _unitOfWork.DebitMemo.ComputeEwtAmount(netOfVatAmount, 0.05m)
                    : 0m;

                #endregion -- Computation --

                #region --General Ledger Book Recording(SV)--

                var ledgers = new List<GeneralLedgerBook>
                {
                    new()
                    {
                        Date = model.TransactionDate,
                        Reference = model.DebitMemoNo!,
                        Description = model.ServiceInvoice.ServiceName,
                        AccountId = arNonTradeTitle.AccountId,
                        AccountNo = arNonTradeTitle.AccountNumber,
                        AccountTitle = arNonTradeTitle.AccountName,
                        Debit = netDiscount - (ewt + wvat),
                        Credit = 0,
                        CreatedBy = model.PostedBy,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = SubAccountType.Customer,
                        SubAccountId = model.ServiceInvoice.CustomerId,
                        SubAccountName = model.ServiceInvoice.CustomerName,
                        ModuleType = nameof(ModuleType.DebitMemo)
                    }
                };

                if (ewt > 0)
                {
                    ledgers.Add(
                        new GeneralLedgerBook
                        {
                            Date = model.TransactionDate,
                            Reference = model.DebitMemoNo!,
                            Description = model.ServiceInvoice.ServiceName,
                            AccountId = arTradeCwt.AccountId,
                            AccountNo = arTradeCwt.AccountNumber,
                            AccountTitle = arTradeCwt.AccountName,
                            Debit = ewt,
                            Credit = 0,
                            CreatedBy = model.PostedBy,
                            CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                            ModuleType = nameof(ModuleType.DebitMemo)
                        }
                    );
                }
                if (wvat > 0)
                {
                    ledgers.Add(
                        new GeneralLedgerBook
                        {
                            Date = model.TransactionDate,
                            Reference = model.DebitMemoNo!,
                            Description = model.ServiceInvoice.ServiceName,
                            AccountId = arTradeCwv.AccountId,
                            AccountNo = arTradeCwv.AccountNumber,
                            AccountTitle = arTradeCwv.AccountName,
                            Debit = wvat,
                            Credit = 0,
                            CreatedBy = model.PostedBy,
                            CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                            ModuleType = nameof(ModuleType.DebitMemo)
                        }
                    );
                }

                if (netOfVatAmount > 0)
                {
                    ledgers.Add(new GeneralLedgerBook
                    {
                        Date = model.TransactionDate,
                        Reference = model.DebitMemoNo!,
                        Description = model.ServiceInvoice.ServiceName,
                        AccountNo = model.ServiceInvoice.Service!.CurrentAndPreviousNo!,
                        AccountTitle = model.ServiceInvoice.Service.CurrentAndPreviousTitle!,
                        Debit = 0,
                        Credit = netOfVatAmount,
                        CreatedBy = model.PostedBy,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.DebitMemo)
                    });
                }

                if (vatAmount > 0)
                {
                    ledgers.Add(
                        new GeneralLedgerBook
                        {
                            Date = model.TransactionDate,
                            Reference = model.DebitMemoNo!,
                            Description = model.ServiceInvoice.ServiceName,
                            AccountId = vatOutputTitle.AccountId,
                            AccountNo = vatOutputTitle.AccountNumber,
                            AccountTitle = vatOutputTitle.AccountName,
                            Debit = 0,
                            Credit = vatAmount,
                            CreatedBy = model.PostedBy,
                            CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                            ModuleType = nameof(ModuleType.DebitMemo)
                        }
                    );
                }

                if (!_unitOfWork.DebitMemo.IsJournalEntriesBalanced(ledgers))
                {
                    throw new ArgumentException("Debit and Credit is not equal, check your entries.");
                }

                await _dbContext.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);

                #endregion --General Ledger Book Recording(SV)--

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.PostedBy!, $"Posted debit memo# {model.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Debit Memo has been Posted.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post debit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.DebitMemo.GetAsync(dm => dm.DebitMemoId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.PostedBy = null;
                model.VoidedBy = GetUserFullName();
                model.VoidedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Voided);

                await _unitOfWork.GeneralLedger.ReverseEntries(model.DebitMemoNo, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!, $"Voided debit memo# {model.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Debit Memo #{model.DebitMemoNo} has been voided successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to void debit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Voided by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.DebitMemo.GetAsync(dm => dm.DebitMemoId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.CancellationRemarks = cancellationRemarks;
                model.Status = nameof(Status.Canceled);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled debit memo# {model.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Debit Memo #{model.DebitMemoNo} has been cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to cancel debit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSVDetails(int svId, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.ServiceInvoice.GetAsync(sv => sv.ServiceInvoiceId == svId, cancellationToken);
            if (model != null)
            {
                return Json(new
                {
                    model.Period,
                    model.Total
                });
            }

            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var debitMemo =
                    await _unitOfWork.DebitMemo.GetAsync(dm => dm.DebitMemoId == id, cancellationToken);

                if (debitMemo == null)
                {
                    return NotFound();
                }

                var minDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.DebitMemo, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.DebitMemo, debitMemo.TransactionDate, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {debitMemo.TransactionDate:MMM yyyy} is already closed.");
                }

                var viewModel = new DebitMemoViewModel
                {
                    DebitMemoId = debitMemo.DebitMemoId,
                    Source = debitMemo.Source,
                    TransactionDate = debitMemo.TransactionDate,
                    ServiceInvoiceId = debitMemo.ServiceInvoiceId,
                    Period = debitMemo.Period,
                    Amount = debitMemo.Amount,
                    Remarks = debitMemo.Remarks,
                    Description = debitMemo.Description,
                    MinDate = minDate,
                };

                await IncludeSelectLists(viewModel, cancellationToken);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch debit memo. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DebitMemoViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(viewModel);
            }

            var model = new DebitMemo
            {
                DebitMemoId = viewModel.DebitMemoId,
                Source = viewModel.Source,
                TransactionDate = viewModel.TransactionDate,
                ServiceInvoiceId = viewModel.ServiceInvoiceId,
                Period = viewModel.Period,
                Amount = viewModel.Amount,
                Remarks = viewModel.Remarks,
                Description = viewModel.Description,
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingDm = await _unitOfWork.DebitMemo
                    .GetAsync(dm => dm.DebitMemoId == model.DebitMemoId, cancellationToken);

                if (existingDm == null)
                {
                    return NotFound();
                }

                existingDm.EditedBy = GetUserFullName();
                existingDm.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingDm.EditedBy!, $"Edited debit memo# {existingDm.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Debit Memo edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                _logger.LogError(ex, "Failed to edit debit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var dm = await _unitOfWork.DebitMemo
                .GetAsync(x => x.DebitMemoId == id, cancellationToken);

            if (dm == null)
            {
                return NotFound();
            }

            if (!dm.IsPrinted)
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(), $"Printed original copy of debit memo# {dm.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                dm.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(), $"Printed re-printed copy of debit memo# {dm.DebitMemoNo}", "Debit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDebitMemoList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                var debitMemos = await _unitOfWork.DebitMemo
                    .GetAllAsync(dm => dm.Type == nameof(DocumentType.Documented), cancellationToken);

                // Apply date range filter if provided
                if (dateFrom.HasValue)
                {
                    debitMemos = debitMemos
                        .Where(s => s.TransactionDate >= DateOnly.FromDateTime(dateFrom.Value))
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    debitMemos = debitMemos
                        .Where(s => s.TransactionDate <= DateOnly.FromDateTime(dateTo.Value))
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    debitMemos = debitMemos
                        .Where(s =>
                            s.DebitMemoNo!.ToLower().Contains(searchValue) ||
                            s.TransactionDate.ToString(SD.Date_Format).ToLower().Contains(searchValue) ||
                            s.ServiceInvoice?.ServiceInvoiceNo?.ToLower().Contains(searchValue) == true ||
                            s.Source!.ToLower().Contains(searchValue) ||
                            s.DebitAmount.ToString().Contains(searchValue) ||
                            s.CreatedBy!.ToLower().Contains(searchValue) ||
                            s.Status.ToLower().Contains(searchValue)
                        )
                        .ToList();
                }

                // Apply sorting if provided
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    debitMemos = debitMemos
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = debitMemos.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<DebitMemo> pagedDebitMemos;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedDebitMemos = debitMemos;
                }
                else
                {
                    // Normal pagination
                    pagedDebitMemos = debitMemos
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedDebitMemos
                    .Select(x => new
                    {
                        x.DebitMemoId,
                        x.DebitMemoNo,
                        x.TransactionDate,
                        serviceInvoiceNo = x.ServiceInvoice?.ServiceInvoiceNo,
                        x.Source,
                        x.DebitAmount,
                        x.CreatedBy,
                        x.Status,
                        // Include status flags for badge rendering
                        isPosted = x.PostedBy != null,
                        isVoided = x.VoidedBy != null,
                        isCanceled = x.CanceledBy != null
                    })
                    .ToList();

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get debit memos. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
