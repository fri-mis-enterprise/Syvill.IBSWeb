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
using OfficeOpenXml;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
    public class CreditMemoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger<CreditMemoController> _logger;

        public CreditMemoController(IUnitOfWork unitOfWork, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<CreditMemoController> logger)
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCreditMemos([FromForm] DataTablesParameters parameters, DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                var creditMemos = _unitOfWork.CreditMemo
                    .GetAllQuery();

                var totalRecords = await creditMemos.CountAsync(cancellationToken);

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasTransactionDate = DateOnly.TryParse(searchValue, out var transactionDate);

                    creditMemos = creditMemos
                    .Where(s =>
                        s.CreditMemoNo!.ToLower().Contains(searchValue) ||
                        s.ServiceInvoice!.ServiceInvoiceNo.ToLower().Contains(searchValue) == true ||
                        (hasTransactionDate && s.TransactionDate == transactionDate) ||
                        s.CreditAmount.ToString().Contains(searchValue) ||
                        s.Remarks!.ToLower().Contains(searchValue) == true ||
                        s.Description.ToLower().Contains(searchValue) ||
                        s.CreatedBy!.ToLower().Contains(searchValue)
                        );
                }
                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    creditMemos = creditMemos.Where(s => s.TransactionDate == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    creditMemos = creditMemos
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await creditMemos.CountAsync(cancellationToken);

                var pagedData = await creditMemos
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
                _logger.LogError(ex, "Failed to get credit memos. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task IncludeSelectLists(CreditMemoViewModel viewModel, CancellationToken cancellationToken)
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

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CreditMemo, cancellationToken);
        }

        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var viewModel = new CreditMemoViewModel();
            await IncludeSelectLists(viewModel, cancellationToken);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreditMemoViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(viewModel);
            }

            var model = new CreditMemo
            {
                Source = viewModel.Source,
                TransactionDate = viewModel.TransactionDate,
                ServiceInvoiceId = viewModel.ServiceInvoiceId,
                Period = viewModel.Period,
                Amount = viewModel.Amount,
                Remarks = viewModel.Remarks,
                Description = viewModel.Description,
            };

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            var existingSv = await _unitOfWork.ServiceInvoice
                        .GetAsync(sv => sv.ServiceInvoiceId == model.ServiceInvoiceId, cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region -- check for unposted DM or CM

                var existingSOADMs = (await _unitOfWork.DebitMemo
                        .GetAllAsync(si => si.ServiceInvoiceId == model.ServiceInvoiceId && si.PostedBy != null && si.CanceledBy != null && si.VoidedBy != null, cancellationToken))
                    .OrderBy(s => s.ServiceInvoiceId)
                    .ToList();
                if (existingSOADMs.Count > 0)
                {
                    await IncludeSelectLists(viewModel, cancellationToken);
                    ModelState.AddModelError("", $"Can’t proceed to create you have unposted DM/CM. {existingSOADMs.First().DebitMemoNo}");
                    return View(viewModel);
                }

                var existingSOACMs = (await _unitOfWork.CreditMemo
                        .GetAllAsync(si => si.ServiceInvoiceId == model.ServiceInvoiceId && si.PostedBy != null && si.CanceledBy != null && si.VoidedBy != null, cancellationToken))
                    .OrderBy(s => s.ServiceInvoiceId)
                    .ToList();
                if (existingSOACMs.Count > 0)
                {
                    await IncludeSelectLists(viewModel, cancellationToken);
                    ModelState.AddModelError("", $"Can’t proceed to create you have unposted DM/CM. {existingSOACMs.First().CreditMemoNo}");
                    return View(viewModel);
                }

                #endregion -- check for unposted DM or CM

                model.CreatedBy = GetUserFullName();

                model.CreditMemoNo = await _unitOfWork.CreditMemo.GenerateCodeAsync(companyClaims, existingSv!.Type, cancellationToken);
                model.Type = existingSv.Type;
                model.CreditAmount = -model.Amount;

                await _unitOfWork.CreditMemo.AddAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!, $"Create new credit memo# {model.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Credit memo #{model.CreditMemoNo} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                _logger.LogError(ex, "Failed to create credit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
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
                var creditMemo =
                    await _unitOfWork.CreditMemo.GetAsync(c => c.CreditMemoId == id, cancellationToken);

                if (creditMemo == null)
                {
                    return NotFound();
                }

                var minDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CreditMemo, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CreditMemo, creditMemo.TransactionDate, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {creditMemo.TransactionDate:MMM yyyy} is already closed.");
                }

                var viewModel = new CreditMemoViewModel
                {
                    CreditMemoId = creditMemo.CreditMemoId,
                    Source = creditMemo.Source,
                    TransactionDate = creditMemo.TransactionDate,
                    ServiceInvoiceId = creditMemo.ServiceInvoiceId,
                    Period = creditMemo.Period,
                    Amount = creditMemo.Amount,
                    Remarks = creditMemo.Remarks,
                    Description = creditMemo.Description,
                    MinDate = minDate,
                };

                await IncludeSelectLists(viewModel, cancellationToken);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch credit memo. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreditMemoViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(viewModel);
            }

            var model = new CreditMemo
            {
                CreditMemoId = viewModel.CreditMemoId,
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
                var existingCm = await _unitOfWork.CreditMemo
                                .GetAsync(cm => cm.CreditMemoId == model.CreditMemoId, cancellationToken);

                if (existingCm == null)
                {
                    return NotFound();
                }

                model.EditedBy = GetUserFullName();
                model.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                switch (model.Source)
                {
                    case "Service Invoice":
                        #region -- Saving Default Enries --

                        existingCm.TransactionDate = model.TransactionDate;
                        existingCm.ServiceInvoiceId = model.ServiceInvoiceId;
                        existingCm.Period = model.Period;
                        existingCm.Amount = model.Amount;
                        existingCm.Description = model.Description;
                        existingCm.Remarks = model.Remarks;

                        #endregion -- Saving Default Enries --

                        existingCm.CreditAmount = -model.Amount;
                        break;
                }

                existingCm.EditedBy = GetUserFullName();
                existingCm.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingCm.EditedBy!, $"Edited credit memo# {existingCm.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Credit Memo edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await IncludeSelectLists(viewModel, cancellationToken);
                _logger.LogError(ex, "Failed to edit credit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
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

            var creditMemo = await _unitOfWork.CreditMemo.GetAsync(c => c.CreditMemoId == id, cancellationToken);

            if (creditMemo == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return NotFound();
            }

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview credit memo# {creditMemo.CreditMemoNo}", "Credit Memo");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(creditMemo);
        }

        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken, ViewModelDMCM viewModelDmcm)
        {
            var model = await _unitOfWork.CreditMemo.GetAsync(c => c.CreditMemoId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CreditMemo, model.TransactionDate, cancellationToken))
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

                    #region --SV Computation--

                    viewModelDmcm.Period = DateOnly.FromDateTime(model.CreatedDate) >= model.Period ? DateOnly.FromDateTime(model.CreatedDate) : model.Period.AddMonths(1).AddDays(-1);

                    if (existingSv!.VatType == SD.VatType_Vatable)
                    {
                        viewModelDmcm.Total = -model.Amount;
                        viewModelDmcm.NetAmount = (model.Amount - existingSv.Discount) / 1.12m;
                        viewModelDmcm.VatAmount = model.Amount - existingSv.Discount - viewModelDmcm.NetAmount;
                        viewModelDmcm.WithholdingTaxAmount = viewModelDmcm.NetAmount * (existingSv.ServicePercent / 100m);
                        if (existingSv.HasWvat)
                        {
                            viewModelDmcm.WithholdingVatAmount = viewModelDmcm.NetAmount * 0.05m;
                        }
                    }
                    else
                    {
                        viewModelDmcm.NetAmount = model.Amount - existingSv.Discount;
                        viewModelDmcm.WithholdingTaxAmount = viewModelDmcm.NetAmount * (existingSv.ServicePercent / 100m);
                        if (existingSv.HasWvat)
                        {
                            viewModelDmcm.WithholdingVatAmount = viewModelDmcm.NetAmount * 0.05m;
                        }
                    }

                    if (existingSv.VatType == "Vatable")
                    {
                        var total = Math.Round(model.Amount / 1.12m, 4);

                        var roundedNetAmount = Math.Round(viewModelDmcm.NetAmount, 4);

                        if (roundedNetAmount > total)
                        {
                            var shortAmount = viewModelDmcm.NetAmount - total;

                            viewModelDmcm.Amount += shortAmount;
                        }
                    }

                    #endregion --SV Computation--

                    #region --General Ledger Book Recording(SV)--

                    decimal withHoldingTaxAmount = 0;
                    decimal withHoldingVatAmount = 0;
                    decimal netOfVatAmount = 0;
                    decimal vatAmount = 0;

                    if (model.ServiceInvoice!.VatType == SD.VatType_Vatable)
                    {
                        netOfVatAmount = (_unitOfWork.CreditMemo.ComputeNetOfVat(Math.Abs(model.CreditAmount))) * -1;
                        vatAmount = (_unitOfWork.CreditMemo.ComputeVatAmount(Math.Abs(netOfVatAmount))) * -1;
                    }
                    else
                    {
                        netOfVatAmount = model.CreditAmount;
                    }

                    if (model.ServiceInvoice.HasEwt)
                    {
                        withHoldingTaxAmount = (_unitOfWork.CreditMemo.ComputeEwtAmount(Math.Abs(netOfVatAmount), 0.01m)) * -1;
                    }

                    if (model.ServiceInvoice.HasWvat)
                    {
                        withHoldingVatAmount = (_unitOfWork.CreditMemo.ComputeEwtAmount(Math.Abs(netOfVatAmount), 0.05m)) * -1;
                    }

                    var ledgers = new List<GeneralLedgerBook>
                    {
                        new()
                        {
                            Date = model.TransactionDate,
                            Reference = model.CreditMemoNo!,
                            Description = model.ServiceInvoice.ServiceName,
                            AccountId = arNonTradeTitle.AccountId,
                            AccountNo = arNonTradeTitle.AccountNumber,
                            AccountTitle = arNonTradeTitle.AccountName,
                            Debit = 0,
                            Credit = Math.Abs(model.CreditAmount - (withHoldingTaxAmount + withHoldingVatAmount)),
                            CreatedBy = model.PostedBy,
                            CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                            SubAccountType = SubAccountType.Customer,
                            SubAccountId = model.ServiceInvoice.CustomerId,
                            SubAccountName = model.ServiceInvoice.CustomerName,
                            ModuleType = nameof(ModuleType.CreditMemo)
                        }
                    };

                    if (withHoldingTaxAmount < 0)
                    {
                        ledgers.Add(
                            new GeneralLedgerBook
                            {
                                Date = model.TransactionDate,
                                Reference = model.CreditMemoNo!,
                                Description = model.ServiceInvoice.ServiceName,
                                AccountId = arTradeCwt.AccountId,
                                AccountNo = arTradeCwt.AccountNumber,
                                AccountTitle = arTradeCwt.AccountName,
                                Debit = 0,
                                Credit = Math.Abs(withHoldingTaxAmount),
                                CreatedBy = model.PostedBy,
                                CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                                ModuleType = nameof(ModuleType.CreditMemo)
                            }
                        );
                    }
                    if (withHoldingVatAmount < 0)
                    {
                        ledgers.Add(
                            new GeneralLedgerBook
                            {
                                Date = model.TransactionDate,
                                Reference = model.CreditMemoNo!,
                                Description = model.ServiceInvoice.ServiceName,
                                AccountId = arTradeCwv.AccountId,
                                AccountNo = arTradeCwv.AccountNumber,
                                AccountTitle = arTradeCwv.AccountName,
                                Debit = 0,
                                Credit = Math.Abs(withHoldingVatAmount),
                                CreatedBy = model.PostedBy,
                                CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                                ModuleType = nameof(ModuleType.CreditMemo)
                            }
                        );
                    }

                    ledgers.Add(new GeneralLedgerBook
                    {
                        Date = model.TransactionDate,
                        Reference = model.CreditMemoNo!,
                        Description = model.ServiceInvoice.ServiceName,
                        AccountNo = model.ServiceInvoice.Service!.CurrentAndPreviousNo!,
                        AccountTitle = model.ServiceInvoice.Service.CurrentAndPreviousTitle!,
                        Debit = viewModelDmcm.NetAmount,
                        Credit = 0,
                        CreatedBy = model.PostedBy,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.CreditMemo)
                    });

                    if (vatAmount < 0)
                    {
                        ledgers.Add(
                            new GeneralLedgerBook
                            {
                                Date = model.TransactionDate,
                                Reference = model.CreditMemoNo!,
                                Description = model.ServiceInvoice.ServiceName,
                                AccountId = vatOutputTitle.AccountId,
                                AccountNo = vatOutputTitle.AccountNumber,
                                AccountTitle = vatOutputTitle.AccountName,
                                Debit = Math.Abs(vatAmount),
                                Credit = 0,
                                CreatedBy = model.PostedBy,
                                CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                                ModuleType = nameof(ModuleType.CreditMemo)
                            }
                        );
                    }

                    if (!_unitOfWork.CreditMemo.IsJournalEntriesBalanced(ledgers))
                    {
                        throw new ArgumentException("Debit and Credit is not equal, check your entries.");
                    }

                    await _dbContext.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);

                    #endregion --General Ledger Book Recording(SV)--

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.PostedBy!, $"Posted credit memo# {model.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Credit Memo has been Posted.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post credit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
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
            var model = await _unitOfWork.CreditMemo.GetAsync(cm => cm.CreditMemoId == id, cancellationToken);

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

                await _unitOfWork.GeneralLedger.ReverseEntries(model.CreditMemoNo, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!, $"Voided credit memo# {model.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Credit Memo #{model.CreditMemoNo} has been voided successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to void credit memo. Voided by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.CreditMemo
                .GetAsync(cm => cm.CreditMemoId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Canceled);
                model.CancellationRemarks = cancellationRemarks;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled credit memo# {model.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Collection Receipt #{model.CreditMemoNo} has been cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to cancel credit memo. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSVDetails(int svId, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.ServiceInvoice.GetAsync(sv => sv.ServiceInvoiceId == svId, cancellationToken);
            if (model == null)
            {
                return Json(null);
            }

            return Json(new
            {
                model.Period,
                model.Total
            });
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var cm = await _unitOfWork.CreditMemo
                .GetAsync(x => x.CreditMemoId == id, cancellationToken);

            if (cm == null)
            {
                return NotFound();
            }

            if (!cm.IsPrinted)
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(), $"Printed original copy of credit memo# {cm.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                cm.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                #region --Audit Trail Recording

                var printedBy = GetUserFullName();
                AuditTrail auditTrailBook = new(printedBy, $"Printed re-printed copy of credit memo# {cm.CreditMemoNo}", "Credit Memo");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCreditMemoList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                var creditMemos = await _unitOfWork.CreditMemo
                    .GetAllAsync(null, cancellationToken);

                // Apply date range filter if provided
                if (dateFrom.HasValue)
                {
                    creditMemos = creditMemos
                        .Where(s => s.TransactionDate >= DateOnly.FromDateTime(dateFrom.Value))
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    creditMemos = creditMemos
                        .Where(s => s.TransactionDate <= DateOnly.FromDateTime(dateTo.Value))
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    creditMemos = creditMemos
                        .Where(s =>
                            s.CreditMemoNo!.ToLower().Contains(searchValue) ||
                            s.TransactionDate.ToString(SD.Date_Format).ToLower().Contains(searchValue) ||
                            s.ServiceInvoice?.ServiceInvoiceNo?.ToLower().Contains(searchValue) == true ||
                            s.Source!.ToLower().Contains(searchValue) ||
                            s.CreditAmount.ToString().Contains(searchValue) ||
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

                    creditMemos = creditMemos
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = creditMemos.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<CreditMemo> pagedCreditMemos;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedCreditMemos = creditMemos;
                }
                else
                {
                    // Normal pagination
                    pagedCreditMemos = creditMemos
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedCreditMemos
                    .Select(x => new
                    {
                        x.CreditMemoId,
                        x.CreditMemoNo,
                        x.TransactionDate,
                        serviceInvoiceNo = x.ServiceInvoice?.ServiceInvoiceNo,
                        x.Source,
                        x.CreditAmount,
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
                _logger.LogError(ex, "Failed to get credit memos. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
