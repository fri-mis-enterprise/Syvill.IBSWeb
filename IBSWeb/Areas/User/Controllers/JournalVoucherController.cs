using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;
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
    [DepartmentAuthorize(SD.Department_Accounting, SD.Department_RCD)]
    public class JournalVoucherController: Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<JournalVoucherController> _logger;

        private const string FilterTypeClaimType = "JournalVoucher.FilterType";

        public JournalVoucherController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork, ILogger<JournalVoucherController> logger)
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

        private async Task UpdateFilterTypeClaim(string filterType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == FilterTypeClaimType);

                if (existingClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, existingClaim);
                }

                if (!string.IsNullOrEmpty(filterType))
                {
                    await _userManager.AddClaimAsync(user, new Claim(FilterTypeClaimType, filterType));
                }
            }
        }

        private async Task<string?> GetCurrentFilterType()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == FilterTypeClaimType)?.Value;
        }

        private async Task<List<SelectListItem>> GetLiquidationCheckVoucherHeadersAsync(int employeeId,
            int? selectedCvId, CancellationToken cancellationToken)
        {
            return await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.IsAdvances &&
                    c.EmployeeId == employeeId &&
                    (c.Status == nameof(CheckVoucherPaymentStatus.Unliquidated) ||
                     c.CheckVoucherHeaderId == selectedCvId))
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<List<SelectListItem>> GetLiquidationProvisionalReceiptsAsync(int employeeId,
            string? selectedPrNo, CancellationToken cancellationToken)
        {
            return await _dbContext.ProvisionalReceipts
                .OrderByDescending(pr => pr.TransactionDate)
                .ThenByDescending(pr => pr.SeriesNumber)
                .Where(pr =>
                    pr.EmployeeId == employeeId &&
                    ((pr.Status != nameof(CollectionReceiptStatus.Canceled) &&
                      pr.Status != nameof(CollectionReceiptStatus.Voided)) ||
                     pr.SeriesNumber == selectedPrNo))
                .Select(pr => new SelectListItem { Value = pr.SeriesNumber, Text = pr.SeriesNumber })
                .ToListAsync(cancellationToken);
        }

        private async Task PopulateLiquidationDependenciesAsync(JournalVoucherViewModel viewModel,
            CancellationToken cancellationToken, int? selectedCvId = null, string? selectedPrNo = null)
        {
            viewModel.COA = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
            viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);
            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            if (viewModel.EmployeeId.HasValue)
            {
                viewModel.CheckVoucherHeaders = await GetLiquidationCheckVoucherHeadersAsync(viewModel.EmployeeId.Value,
                    selectedCvId ?? viewModel.CVId, cancellationToken);
                viewModel.ProvisionalReceipts = await GetLiquidationProvisionalReceiptsAsync(viewModel.EmployeeId.Value,
                    selectedPrNo ?? viewModel.PRNo, cancellationToken);
                return;
            }

            viewModel.CheckVoucherHeaders = new List<SelectListItem>();
            viewModel.ProvisionalReceipts = new List<SelectListItem>();
        }

        public async Task<IActionResult> Index(string filterType)
        {
            await UpdateFilterTypeClaim(filterType);
            ViewBag.FilterType = await GetCurrentFilterType();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetJournalVouchers([FromForm] DataTablesParameters parameters,
            DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {
                var filterTypeClaim = await GetCurrentFilterType();

                var journalVoucherHeader = _unitOfWork.JournalVoucher
                    .GetAllQuery();

                var totalRecords = await journalVoucherHeader.CountAsync(cancellationToken);

                // Apply status filter based on filterType
                if (!string.IsNullOrEmpty(filterTypeClaim))
                {
                    switch (filterTypeClaim)
                    {
                        case "ForApproval":
                            journalVoucherHeader =
                                journalVoucherHeader.Where(jv => jv.Status == nameof(JvStatus.ForApproval));
                            break;
                    }
                }

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasDate = DateOnly.TryParse(searchValue, out var date);

                    journalVoucherHeader = journalVoucherHeader
                        .Where(s =>
                            s.JournalVoucherHeaderNo!.ToLower().Contains(searchValue) ||
                            (hasDate && s.Date == date) ||
                            s.References!.ToLower().Contains(searchValue) == true ||
                            s.CheckVoucherHeader!.CheckVoucherHeaderNo!.ToLower().Contains(searchValue) == true ||
                            s.Particulars.ToLower().Contains(searchValue) ||
                            (s.CRNo != null && s.CRNo.ToLower().Contains(searchValue)) ||
                            s.JVReason.ToLower().Contains(searchValue) ||
                            s.CreatedBy!.ToLower().Contains(searchValue)
                        );
                }

                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    journalVoucherHeader = journalVoucherHeader.Where(s => s.Date == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    journalVoucherHeader = journalVoucherHeader
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await journalVoucherHeader.CountAsync(cancellationToken);

                var pagedData = await journalVoucherHeader
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
                _logger.LogError(ex, "Failed to get journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateLiquidation(CancellationToken cancellationToken)
        {
            var viewModel = new JournalVoucherViewModel();

            await PopulateLiquidationDependenciesAsync(viewModel, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLiquidation(JournalVoucherViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await PopulateLiquidationDependenciesAsync(viewModel, cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving the default entries

                var generateJvNo =
                    await _unitOfWork.JournalVoucher.GenerateCodeAsync(viewModel.Type,
                        cancellationToken: cancellationToken);
                //JV Header Entry
                var model = new JournalVoucherHeader
                {
                    Type = viewModel.Type!,
                    JournalVoucherHeaderNo = generateJvNo,
                    Date = viewModel.TransactionDate,
                    References = viewModel.References,
                    CVId = viewModel.CVId,
                    Particulars = viewModel.Particulars,
                    CRNo = viewModel.PRNo,
                    JVReason = viewModel.JVReason,
                    CreatedBy = GetUserFullName(),
                    JvType = nameof(JvType.Liquidation)
                };

                await _dbContext.AddAsync(model, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                #endregion --Saving the default entries

                #region Details

                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == model.CVId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {model.CVId} not found");

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var detail in viewModel.Details!)
                {
                    var currentAccountNumber = detail.AccountNumber;
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == currentAccountNumber,
                                               cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {currentAccountNumber} not found");

                    var isAdvances = accountTitle.AccountName.Contains("Advances to Officers and Employees");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = currentAccountNumber,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = generateJvNo,
                            JournalVoucherHeaderId = model.JournalVoucherHeaderId,
                            Debit = detail.Debit,
                            Credit = detail.Credit,
                            SubAccountType =
                                isAdvances ? SubAccountType.Employee :
                                detail.SubAccountId != null ? SubAccountType.Supplier : null,
                            SubAccountId = isAdvances ? cv.EmployeeId : detail.SubAccountId,
                            SubAccountName = isAdvances ? cv.Payee : detail.SubAccountCodeName,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy,
                    $"Created new journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Journal voucher # {model.JournalVoucherHeaderNo} created successfully";

                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> GetCV(int id, CancellationToken cancellationToken)
        {
            var model = await _dbContext.CheckVoucherHeaders
                .Include(s => s.Supplier)
                .Include(cvd => cvd.Details)
                .FirstOrDefaultAsync(cvh => cvh.CheckVoucherHeaderId == id, cancellationToken);

            if (model != null)
            {
                return Json(new
                {
                    CVNo = model.CheckVoucherHeaderNo,
                    model.Date,
                    Name = model.SupplierName,
                    Address = model.Address,
                    TinNo = model.Tin,
                    model.PONo,
                    model.SINo,
                    model.Payee,
                    Amount = model.Total,
                    model.Particulars,
                    model.CheckNo,
                    AccountNo = model.Details!.Select(jvd => jvd.AccountNo),
                    AccountName = model.Details!.Select(jvd => jvd.AccountName),
                    Debit = model.Details!.Select(jvd => jvd.Debit),
                    Credit = model.Details!.Select(jvd => jvd.Credit),
                    TotalDebit = model.Details!.Sum(cvd => cvd.Debit),
                    TotalCredit = model.Details!.Sum(cvd => cvd.Credit),
                });
            }

            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiquidationCheckVouchersByEmployee(int employeeId, int? selectedCvId,
            CancellationToken cancellationToken = default)
        {
            var selectList = await GetLiquidationCheckVoucherHeadersAsync(employeeId, selectedCvId, cancellationToken);
            return Json(selectList);
        }

        [HttpGet]
        public async Task<IActionResult> GetProvisionalReceiptsByEmployee(int employeeId, string? selectedPrNo,
            CancellationToken cancellationToken = default)
        {
            var selectList = await GetLiquidationProvisionalReceiptsAsync(employeeId, selectedPrNo, cancellationToken);
            return Json(selectList);
        }

        [HttpGet]
        public async Task<IActionResult> Print(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            var header = await _dbContext.JournalVoucherHeaders
                .Include(cv => cv.CheckVoucherHeader)
                .ThenInclude(supplier => supplier!.Supplier)
                .Include(jv => jv.Details)
                .FirstOrDefaultAsync(jvh => jvh.JournalVoucherHeaderId == id.Value, cancellationToken);

            if (header == null)
            {
                return NotFound();
            }

            var viewModel = new JournalVoucherVM { Header = header, Details = header.Details!.ToList(), };

            viewModel.IsAmortization = await _dbContext
                .JvAmortizationSettings
                .AnyAsync(jv => jv.JvId == id.Value && jv.IsActive, cancellationToken);

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(),
                $"Preview journal voucher# {header.JournalVoucherHeaderNo}", "Journal Voucher");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            ViewBag.FilterType = await GetCurrentFilterType();
            return View(viewModel);
        }

        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var modelHeader = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == id, cancellationToken);

                if (modelHeader == null)
                {
                    return NotFound();
                }

                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, modelHeader.Date, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot post this record because the period {modelHeader.Date:MMM yyyy} is already closed.");
                }

                var modelDetails = await _dbContext.JournalVoucherDetails
                    .Where(jvd => jvd.JournalVoucherHeaderId == modelHeader.JournalVoucherHeaderId)
                    .ToListAsync(cancellationToken: cancellationToken);

                modelHeader.PostedBy = GetUserFullName();
                modelHeader.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                modelHeader.Status = nameof(Status.Posted);

                if (modelHeader.JvType == nameof(JvType.Accrual))
                {
                    await ReverseAccrual(modelHeader.JournalVoucherHeaderId, cancellationToken);
                }

                await _unitOfWork.JournalVoucher.PostAsync(modelHeader, modelDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(modelHeader.PostedBy!,
                    $"Posted journal voucher# {modelHeader.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher has been Posted.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to post journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
        {
            var model = await _dbContext.JournalVoucherHeaders
                .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == id, cancellationToken);

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
                model.Status = nameof(JvStatus.Voided);

                await _unitOfWork.GeneralLedger.ReverseEntries(model.JournalVoucherHeaderNo, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!,
                    $"Voided journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message = $"Journal Voucher #{model.JournalVoucherHeaderNo} has been voided successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to void journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var model = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == id, cancellationToken);

                if (model == null)
                {
                    return NotFound();
                }

                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, model.Date, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot cancel this record because the period {model.Date:MMM yyyy} is already closed.");
                }

                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(JvStatus.Canceled);
                model.CancellationRemarks = cancellationRemarks;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CanceledBy!,
                    $"Canceled journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message = $"Journal Voucher #{model.JournalVoucherHeaderNo} has been cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Failed to cancel journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditLiquidation(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .Include(jv => jv.CheckVoucherHeader)
                    .FirstOrDefaultAsync(cvh => cvh.JournalVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }

                var existingDetailsModel = await _dbContext.JournalVoucherDetails
                    .Where(cvd => cvd.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                JournalVoucherViewModel model = new()
                {
                    JVId = existingHeaderModel.JournalVoucherHeaderId,
                    JVNo = existingHeaderModel.JournalVoucherHeaderNo,
                    TransactionDate = existingHeaderModel.Date,
                    References = existingHeaderModel.References,
                    CVId = existingHeaderModel.CVId,
                    EmployeeId = existingHeaderModel.CheckVoucherHeader?.EmployeeId,
                    Particulars = existingHeaderModel.Particulars,
                    PRNo = existingHeaderModel.CRNo,
                    JVReason = existingHeaderModel.JVReason,
                    Details = existingDetailsModel.Select(d => new JournalVoucherDetailViewModel
                    {
                        AccountNumber = d.AccountNo,
                        AccountTitle = d.AccountName,
                        Debit = d.Debit,
                        Credit = d.Credit,
                        SubAccountId = d.SubAccountId,
                        SubAccountCodeName = d.SubAccountName
                    }).ToList(),
                    MinDate = minDate
                };

                await PopulateLiquidationDependenciesAsync(model, cancellationToken, existingHeaderModel.CVId,
                    existingHeaderModel.CRNo);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch jv. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLiquidation(JournalVoucherViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await PopulateLiquidationDependenciesAsync(viewModel, cancellationToken, viewModel.CVId, viewModel.PRNo);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == viewModel.JVId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                await _dbContext.JournalVoucherDetails
                    .Where(d => d.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                #region --Saving the default entries

                existingHeaderModel.JournalVoucherHeaderNo = viewModel.JVNo!;
                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.References = viewModel.References;
                existingHeaderModel.CVId = viewModel.CVId;
                existingHeaderModel.Particulars = viewModel.Particulars;
                existingHeaderModel.CRNo = viewModel.PRNo;
                existingHeaderModel.JVReason = viewModel.JVReason;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                #endregion --Saving the default entries

                #region Details

                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == existingHeaderModel.CVId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {existingHeaderModel.CVId} not found");

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var detail in viewModel.Details!)
                {
                    var currentAccountNumber = detail.AccountNumber;
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == currentAccountNumber,
                                               cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {currentAccountNumber} not found");

                    var isAdvances = accountTitle.AccountName.Contains("Advances to Officers and Employees");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = currentAccountNumber,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = existingHeaderModel.JournalVoucherHeaderNo!,
                            JournalVoucherHeaderId = existingHeaderModel.JournalVoucherHeaderId,
                            Debit = detail.Debit,
                            Credit = detail.Credit,
                            SubAccountType =
                                isAdvances ? SubAccountType.Employee :
                                detail.SubAccountId != null ? SubAccountType.Supplier : null,
                            SubAccountId = isAdvances ? cv.EmployeeId : detail.SubAccountId,
                            SubAccountName = isAdvances ? cv.Payee : detail.SubAccountCodeName,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingHeaderModel.EditedBy!,
                    $"Edited journal voucher# {existingHeaderModel.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken); // await the SaveChangesAsync method
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher edited successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var cv = await _unitOfWork.JournalVoucher.GetAsync(x => x.JournalVoucherHeaderId == id, cancellationToken);

            if (cv == null)
            {
                return NotFound();
            }

            if (cv.IsPrinted == false)
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Printed original copy of journal voucher# {cv.JournalVoucherHeaderNo}", "Journal Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                cv.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Printed re-printed copy of journal voucher# {cv.JournalVoucherHeaderNo}", "Journal Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetJournalVoucherList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var journalVoucherHeaders = await _unitOfWork.JournalVoucher
                    .GetAllAsync(jv => jv.Type == nameof(DocumentType.Documented), cancellationToken);

                // Apply firstDayOfNextMonth range filter if provided
                if (dateFrom.HasValue)
                {
                    journalVoucherHeaders = journalVoucherHeaders
                        .Where(s => s.Date >= DateOnly.FromDateTime(dateFrom.Value))
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    journalVoucherHeaders = journalVoucherHeaders
                        .Where(s => s.Date <= DateOnly.FromDateTime(dateTo.Value))
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    journalVoucherHeaders = journalVoucherHeaders
                        .Where(s =>
                            (s.JournalVoucherHeaderNo != null &&
                             s.JournalVoucherHeaderNo.ToLower().Contains(searchValue)) ||
                            s.Date.ToString(SD.Date_Format).ToLower().Contains(searchValue) ||
                            (s.CheckVoucherHeader?.CheckVoucherHeaderNo != null && s.CheckVoucherHeader
                                .CheckVoucherHeaderNo.ToLower().Contains(searchValue)) ||
                            (s.CRNo != null && s.CRNo.ToLower().Contains(searchValue)) ||
                            (s.JVReason != null && s.JVReason.ToLower().Contains(searchValue)) ||
                            (s.CreatedBy != null && s.CreatedBy.ToLower().Contains(searchValue)) ||
                            s.Status.ToLower().Contains(searchValue)
                        )
                        .ToList();
                }

                // Apply sorting if provided
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    // Map frontend column names to actual entity property names
                    var columnMapping = new Dictionary<string, string>
                    {
                        { "journalVoucherHeaderNo", "JournalVoucherHeaderNo" },
                        { "date", "Date" },
                        { "checkVoucherHeaderNo", "CheckVoucherHeader.CheckVoucherHeaderNo" },
                        { "crNo", "CRNo" },
                        { "jvReason", "JVReason" },
                        { "createdBy", "CreatedBy" },
                        { "status", "Status" }
                    };

                    // Get the actual property name
                    var actualColumnName = columnMapping.ContainsKey(columnName)
                        ? columnMapping[columnName]
                        : columnName;

                    journalVoucherHeaders = journalVoucherHeaders
                        .AsQueryable()
                        .OrderBy($"{actualColumnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = journalVoucherHeaders.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<JournalVoucherHeader> pagedJournalVoucherHeaders;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedJournalVoucherHeaders = journalVoucherHeaders;
                }
                else
                {
                    // Normal pagination
                    pagedJournalVoucherHeaders = journalVoucherHeaders
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedJournalVoucherHeaders
                    .Select(x => new
                    {
                        x.JournalVoucherHeaderId,
                        x.JournalVoucherHeaderNo,
                        x.Date,
                        checkVoucherHeaderNo = x.CheckVoucherHeader?.CheckVoucherHeaderNo,
                        x.CRNo,
                        x.JVReason,
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
                _logger.LogError(ex, "Failed to get journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReJournalJv(int? month, int? year, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var jvs = await _dbContext.JournalVoucherHeaders
                    .Include(x => x.Details)
                    .Where(x =>
                        x.PostedBy != null &&
                        x.Date.Month == month &&
                        x.Date.Year == year)
                    .ToListAsync(cancellationToken);

                if (!jvs.Any())
                {
                    return Json(new { sucess = true, message = "No records were returned." });
                }

                foreach (var jv in jvs
                             .OrderBy(x => x.Date))
                {
                    await _unitOfWork.JournalVoucher.PostAsync(jv,
                        jv.Details!,
                        cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Json(new { month, year, count = jvs.Count });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccrual(CancellationToken cancellationToken)
        {
            var viewModel = new JvCreateAccrualViewModel();

            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccrual(JvCreateAccrualViewModel viewModel,
            CancellationToken cancellationToken)
        {
            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == viewModel.CvId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {viewModel.CvId} not found");

                #region --Saving the default entries

                var generateJvNo =
                    await _unitOfWork.JournalVoucher.GenerateCodeAsync(cv.Type, cancellationToken: cancellationToken);
                var expenseTitle = string.Join(" ",
                    viewModel.Details.First(d => d.Debit > 0).AccountTitle.Split(' ').Skip(1));
                var particulars = $"Accrual of '{expenseTitle}' for the month of {viewModel.TransactionDate:MMM yyyy}.";
                var model = new JournalVoucherHeader
                {
                    Type = cv.Type,
                    JournalVoucherHeaderNo = generateJvNo,
                    Date = viewModel.TransactionDate,
                    References = viewModel.References,
                    CVId = viewModel.CvId,
                    Particulars = particulars,
                    CRNo = viewModel.CrNo,
                    JVReason = viewModel.Reason,
                    CreatedBy = GetUserFullName(),
                    JvType = nameof(JvType.Accrual)
                };

                await _dbContext.AddAsync(model, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    var isAccrualAccount = accountTitle.AccountName.Contains("AP - Accrued Expenses");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = generateJvNo,
                            JournalVoucherHeaderId = model.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                            SubAccountType = isAccrualAccount ? SubAccountType.Supplier : null,
                            SubAccountId = isAccrualAccount ? cv.SupplierId : null,
                            SubAccountName = isAccrualAccount ? cv.Payee : null,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy,
                    $"Created new journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Journal voucher # {model.JournalVoucherHeaderNo} created successfully";

                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAccrual(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .Include(jv => jv.CheckVoucherHeader)
                    .FirstOrDefaultAsync(cvh => cvh.JournalVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }

                var existingDetailsModel = await _dbContext.JournalVoucherDetails
                    .Where(cvd => cvd.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                JvEditAccrualViewModel model = new()
                {
                    JvId = existingHeaderModel.JournalVoucherHeaderId,
                    TransactionDate = existingHeaderModel.Date,
                    References = existingHeaderModel.References,
                    CvId = (int)existingHeaderModel.CVId!,
                    CrNo = existingHeaderModel.CRNo,
                    Reason = existingHeaderModel.JVReason,
                    CvList = await _dbContext.CheckVoucherHeaders
                        .OrderBy(c => c.CheckVoucherHeaderNo)
                        .Where(c =>
                            c.CvType == nameof(CVType.Invoicing) &&
                            c.PostedBy != null)
                        .Select(cvh => new SelectListItem
                        {
                            Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                        })
                        .ToListAsync(cancellationToken),
                    MinDate = minDate
                };

                foreach (var detail in existingDetailsModel)
                {
                    model.Details.Add(new JvEditAccrualDetailViewModel
                    {
                        AccountNo = detail.AccountNo,
                        AccountTitle = detail.AccountName,
                        Debit = detail.Debit,
                        Credit = detail.Credit,
                    });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch JV. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccrual(JvEditAccrualViewModel viewModel,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == viewModel.JvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == viewModel.CvId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {viewModel.CvId} not found");

                await _dbContext.JournalVoucherDetails
                    .Where(d => d.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                #region --Saving the default entries

                var expenseTitle = string.Join(" ",
                    viewModel.Details.First(d => d.Debit > 0).AccountTitle.Split(' ').Skip(1));
                var particulars = $"Accrual of '{expenseTitle}' for the month of {viewModel.TransactionDate:MMM yyyy}.";

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.References = viewModel.References;
                existingHeaderModel.CVId = viewModel.CvId;
                existingHeaderModel.Particulars = particulars;
                existingHeaderModel.CRNo = viewModel.CrNo;
                existingHeaderModel.JVReason = viewModel.Reason;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.Status = nameof(JvStatus.ForApproval);

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    var isAccrualAccount = accountTitle.AccountName.Contains("AP - Accrued Expenses");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = existingHeaderModel.JournalVoucherHeaderNo!,
                            JournalVoucherHeaderId = existingHeaderModel.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                            SubAccountType = isAccrualAccount ? SubAccountType.Supplier : null,
                            SubAccountId = isAccrualAccount ? cv.SupplierId : null,
                            SubAccountName = isAccrualAccount ? cv.Payee : null,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingHeaderModel.EditedBy,
                    $"Edited journal voucher# {existingHeaderModel.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher edited successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        private async Task ReverseAccrual(int id, CancellationToken cancellationToken)
        {
            var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                                          .Include(x => x.Details)
                                          .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == id, cancellationToken)
                                      ?? throw new InvalidOperationException($"Journal voucher header {id} not found.");

            var accountTitlesDto = await _unitOfWork.JournalVoucher.GetListOfAccountTitleDto(cancellationToken);
            var ledgers = new List<GeneralLedgerBook>();
            var nextMonth = existingHeaderModel.Date.AddMonths(1);
            var firstDayOfNextMonth = new DateOnly(nextMonth.Year, nextMonth.Month, 1);

            foreach (var detail in existingHeaderModel.Details!)
            {
                var account = accountTitlesDto.Find(c => c.AccountNumber == detail.AccountNo)
                              ?? throw new ArgumentException($"Account title '{detail.AccountNo}' not found.");

                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = firstDayOfNextMonth,
                        Reference = existingHeaderModel.JournalVoucherHeaderNo!,
                        Description = $"Reversal of {existingHeaderModel.Particulars}",
                        AccountId = account.AccountId,
                        AccountNo = account.AccountNumber,
                        AccountTitle = account.AccountName,
                        Debit = detail.Credit,
                        Credit = detail.Debit,
                        CreatedBy = existingHeaderModel.CreatedBy!,
                        CreatedDate = existingHeaderModel.CreatedDate,
                        SubAccountType = detail.SubAccountType,
                        SubAccountId = detail.SubAccountId,
                        SubAccountName = detail.SubAccountName,
                        ModuleType = nameof(ModuleType.Journal)
                    }
                );
            }

            if (!_unitOfWork.JournalVoucher.IsJournalEntriesBalanced(ledgers))
            {
                throw new ArgumentException("Debit and Credit is not equal, check your entries.");
            }

            await _dbContext.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAmortization(CancellationToken cancellationToken)
        {
            var viewModel = new JvCreateAmortizationViewModel();

            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.PrepaidExpenseAccounts = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountName.Contains("Prepaid Expenses") && !coa.HasChildren)
                .Select(coa => new SelectListItem
                {
                    Value = coa.AccountNumber, Text = $"{coa.AccountNumber} - {coa.AccountName}"
                })
                .ToListAsync(cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAmortization(JvCreateAmortizationViewModel viewModel,
            CancellationToken cancellationToken)
        {
            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.PrepaidExpenseAccounts = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountName.Contains("Prepaid Expenses") && !coa.HasChildren)
                .Select(coa => new SelectListItem
                {
                    Value = coa.AccountNumber, Text = $"{coa.AccountNumber} - {coa.AccountName}"
                })
                .ToListAsync(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == viewModel.CvId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {viewModel.CvId} not found");

                #region --Saving the default entries

                var generateJvNo =
                    await _unitOfWork.JournalVoucher.GenerateCodeAsync(cv.Type, cancellationToken: cancellationToken);
                var startingMonth = new DateOnly(viewModel.TransactionDate.Year, viewModel.TransactionDate.Month, 1);
                var endingMonth = startingMonth.AddMonths(viewModel.NumberOfMonths - 1);
                var expenseAccount = viewModel.Details.First(d => d.Debit > 0).AccountTitle;
                var prepaidAccount = viewModel.Details.First(d => d.Credit > 0).AccountTitle;
                var expenseTitle = string.Join(" ", expenseAccount.Split(' ').Skip(1));

                var particulars =
                    $"Amortization of '{expenseTitle}' from {startingMonth:MMM yyyy} to {endingMonth:MMM yyyy}.";
                var model = new JournalVoucherHeader
                {
                    Type = cv.Type,
                    JournalVoucherHeaderNo = generateJvNo,
                    Date = viewModel.TransactionDate,
                    References = viewModel.References,
                    CVId = viewModel.CvId,
                    Particulars = particulars,
                    CRNo = viewModel.CrNo,
                    JVReason = viewModel.Reason,
                    CreatedBy = GetUserFullName(),
                    JvType = nameof(JvType.Amortization)
                };

                await _dbContext.AddAsync(model, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var amortizationSetting = new JvAmortizationSetting
                {
                    JvId = model.JournalVoucherHeaderId,
                    StartDate = startingMonth,
                    EndDate = endingMonth,
                    OccurrenceTotal = viewModel.NumberOfMonths,
                    OccurrenceRemaining = viewModel.NumberOfMonths - 1,
                    IsActive = true,
                    ExpenseAccount = expenseAccount,
                    PrepaidAccount = prepaidAccount,
                };

                await _dbContext.AddAsync(amortizationSetting, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    var isPrepaidAccount = accountTitle.AccountName.Contains("Prepaid Expenses");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = generateJvNo,
                            JournalVoucherHeaderId = model.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                            SubAccountType = isPrepaidAccount ? SubAccountType.Supplier : null,
                            SubAccountId = isPrepaidAccount ? cv.SupplierId : null,
                            SubAccountName = isPrepaidAccount ? cv.Payee : null,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy,
                    $"Created new journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Journal voucher # {model.JournalVoucherHeaderNo} created successfully";

                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAmortization(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existingAmortizationSetting = await _dbContext.JvAmortizationSettings
                    .Include(jv => jv.JvHeader)
                    .ThenInclude(jv => jv.CheckVoucherHeader)
                    .FirstOrDefaultAsync(jv => jv.JvId == id, cancellationToken);

                if (existingAmortizationSetting == null)
                {
                    TempData["info"] =
                        "This record cannot be edited because it is automatically generated based on the amortization settings.";
                    return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
                }

                var header = existingAmortizationSetting.JvHeader;

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, header.Date, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {header.Date:MMM yyyy} is already closed.");
                }

                var existingDetailsModel = await _dbContext.JournalVoucherDetails
                    .Where(cvd => cvd.JournalVoucherHeaderId == header.JournalVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                JvEditAmortizationViewModel model = new()
                {
                    JvId = header.JournalVoucherHeaderId,
                    TransactionDate = header.Date,
                    References = header.References,
                    CvId = (int)header.CVId!,
                    CrNo = header.CRNo,
                    Reason = header.JVReason,
                    MinDate = minDate,
                    SelectedExpenseAccount = existingAmortizationSetting.ExpenseAccount.Split(" ")[0],
                    SelectedPrepaidAccount = existingAmortizationSetting.PrepaidAccount.Split(" ")[0],
                    NumberOfMonths = existingAmortizationSetting.OccurrenceTotal
                };

                model.CvList = await _dbContext.CheckVoucherHeaders
                    .OrderBy(c => c.CheckVoucherHeaderNo)
                    .Where(c =>
                        c.CvType == nameof(CVType.Invoicing) &&
                        c.PostedBy != null)
                    .Select(cvh => new SelectListItem
                    {
                        Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                    })
                    .ToListAsync(cancellationToken);

                model.MinDate = await _unitOfWork
                    .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

                model.PrepaidExpenseAccounts = await _dbContext.ChartOfAccounts
                    .Where(coa => coa.AccountName.Contains("Prepaid Expenses") && !coa.HasChildren)
                    .Select(coa => new SelectListItem
                    {
                        Value = coa.AccountNumber, Text = $"{coa.AccountNumber} - {coa.AccountName}"
                    })
                    .ToListAsync(cancellationToken);

                foreach (var detail in existingDetailsModel)
                {
                    model.Details.Add(new JvEditAmortizationDetailViewModel
                    {
                        AccountNo = detail.AccountNo,
                        AccountTitle = detail.AccountName,
                        Debit = detail.Debit,
                        Credit = detail.Credit
                    });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch JV. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAmortization(JvEditAmortizationViewModel viewModel,
            CancellationToken cancellationToken)
        {
            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    c.CvType == nameof(CVType.Invoicing) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.PrepaidExpenseAccounts = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountName.Contains("Prepaid Expenses") && !coa.HasChildren)
                .Select(coa => new SelectListItem
                {
                    Value = coa.AccountNumber, Text = $"{coa.AccountNumber} - {coa.AccountName}"
                })
                .ToListAsync(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == viewModel.JvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var cv = await _unitOfWork.CheckVoucher
                             .GetAsync(x => x.CheckVoucherHeaderId == viewModel.CvId, cancellationToken)
                         ?? throw new NullReferenceException($"CV id {viewModel.CvId} not found");

                var amortizationSetting = await _dbContext.JvAmortizationSettings
                                              .FirstOrDefaultAsync(
                                                  jv => jv.JvId == existingHeaderModel.JournalVoucherHeaderId &&
                                                        jv.IsActive, cancellationToken)
                                          ?? throw new NullReferenceException(
                                              $"JV#{existingHeaderModel.JournalVoucherHeaderId} amortization settings not found.");

                await _dbContext.JournalVoucherDetails
                    .Where(d => d.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                #region --Saving the default entries

                var startingMonth = new DateOnly(viewModel.TransactionDate.Year, viewModel.TransactionDate.Month, 1);
                var endingMonth = startingMonth.AddMonths(viewModel.NumberOfMonths - 1);
                var expenseAccount = viewModel.Details.First(d => d.Debit > 0).AccountTitle;
                var prepaidAccount = viewModel.Details.First(d => d.Credit > 0).AccountTitle;
                var expenseTitle = string.Join(" ", expenseAccount.Split(' ').Skip(1));

                var particulars =
                    $"Amortization of '{expenseTitle}' from {startingMonth:MMM yyyy} to {endingMonth:MMM yyyy}.";

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.References = viewModel.References;
                existingHeaderModel.CVId = viewModel.CvId;
                existingHeaderModel.Particulars = particulars;
                existingHeaderModel.CRNo = viewModel.CrNo;
                existingHeaderModel.JVReason = viewModel.Reason;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                amortizationSetting.StartDate = startingMonth;
                amortizationSetting.EndDate = endingMonth;
                amortizationSetting.OccurrenceTotal = viewModel.NumberOfMonths;
                amortizationSetting.OccurrenceRemaining = viewModel.NumberOfMonths - 1;
                amortizationSetting.IsActive = true;
                amortizationSetting.ExpenseAccount = expenseAccount;
                amortizationSetting.PrepaidAccount = prepaidAccount;

                await _dbContext.SaveChangesAsync(cancellationToken);

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    var isPrepaidAccount = accountTitle.AccountName.Contains("Prepaid Expenses");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = existingHeaderModel.JournalVoucherHeaderNo!,
                            JournalVoucherHeaderId = existingHeaderModel.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                            SubAccountType = isPrepaidAccount ? SubAccountType.Supplier : null,
                            SubAccountId = isPrepaidAccount ? cv.SupplierId : null,
                            SubAccountName = isPrepaidAccount ? cv.Payee : null,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingHeaderModel.EditedBy,
                    $"Edited journal voucher# {existingHeaderModel.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher edited successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateReclass(CancellationToken cancellationToken)
        {
            var viewModel = new JvCreateReclassViewModel();

            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    (c.CvType == nameof(CVType.Invoicing) || c.CvType == nameof(CVType.Payment)) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.CoaList = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReclass(JvCreateReclassViewModel viewModel,
            CancellationToken cancellationToken)
        {
            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    (c.CvType == nameof(CVType.Invoicing) || c.CvType == nameof(CVType.Payment)) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.CoaList = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving the default entries

                var generateJvNo =
                    await _unitOfWork.JournalVoucher.GenerateCodeAsync(viewModel.Type,
                        cancellationToken: cancellationToken);
                var model = new JournalVoucherHeader
                {
                    Type = viewModel.Type,
                    JournalVoucherHeaderNo = generateJvNo,
                    Date = viewModel.TransactionDate,
                    References = viewModel.References,
                    CVId = viewModel.CvId,
                    Particulars = viewModel.Particulars,
                    CRNo = viewModel.CrNo,
                    JVReason = viewModel.Reason,
                    CreatedBy = GetUserFullName(),
                    JvType = nameof(JvType.Reclass)
                };

                await _dbContext.AddAsync(model, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = generateJvNo,
                            JournalVoucherHeaderId = model.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy,
                    $"Created new journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Journal voucher # {model.JournalVoucherHeaderNo} created successfully";

                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditReclass(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .Include(jv => jv.Details)
                    .FirstOrDefaultAsync(jv => jv.JournalVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }

                JvEditReclassViewModel model = new()
                {
                    JvId = existingHeaderModel.JournalVoucherHeaderId,
                    TransactionDate = existingHeaderModel.Date,
                    References = existingHeaderModel.References,
                    CvId = existingHeaderModel.CVId,
                    Particulars = existingHeaderModel.Particulars,
                    CrNo = existingHeaderModel.CRNo,
                    Reason = existingHeaderModel.JVReason,
                    CvList = await _dbContext.CheckVoucherHeaders
                        .OrderBy(c => c.CheckVoucherHeaderNo)
                        .Where(c =>
                            (c.CvType == nameof(CVType.Invoicing) || c.CvType == nameof(CVType.Payment)) &&
                            c.PostedBy != null)
                        .Select(cvh => new SelectListItem
                        {
                            Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                        })
                        .ToListAsync(cancellationToken),
                    CoaList = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken),
                    MinDate = minDate,
                    Details = existingHeaderModel.Details!
                        .Select(d => new JvEditReclassDetailViewModel
                        {
                            AccountNo = d.AccountNo,
                            AccountTitle = $"{d.AccountNo} {d.AccountName}",
                            Debit = d.Debit,
                            Credit = d.Credit
                        })
                        .ToList(),
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch jv. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReclass(JvEditReclassViewModel viewModel,
            CancellationToken cancellationToken)
        {
            viewModel.CvList = await _dbContext.CheckVoucherHeaders
                .OrderBy(c => c.CheckVoucherHeaderNo)
                .Where(c =>
                    (c.CvType == nameof(CVType.Invoicing) || c.CvType == nameof(CVType.Payment)) &&
                    c.PostedBy != null)
                .Select(cvh => new SelectListItem
                {
                    Value = cvh.CheckVoucherHeaderId.ToString(), Text = cvh.CheckVoucherHeaderNo
                })
                .ToListAsync(cancellationToken);

            viewModel.MinDate = await _unitOfWork
                .GetMinimumPeriodBasedOnThePostedPeriods(Module.JournalVoucher, cancellationToken);

            viewModel.CoaList = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingHeaderModel = await _dbContext.JournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderId == viewModel.JvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                await _dbContext.JournalVoucherDetails
                    .Where(d => d.JournalVoucherHeaderId == existingHeaderModel.JournalVoucherHeaderId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                #region --Saving the default entries

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.References = viewModel.References;
                existingHeaderModel.CVId = viewModel.CvId;
                existingHeaderModel.Particulars = viewModel.Particulars;
                existingHeaderModel.CRNo = viewModel.CrNo;
                existingHeaderModel.JVReason = viewModel.Reason;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                #endregion --Saving the default entries

                #region Details

                var jvDetails = new List<JournalVoucherDetail>();

                foreach (var acctNo in viewModel.Details)
                {
                    var accountTitle = await _unitOfWork.ChartOfAccount
                                           .GetAsync(coa => coa.AccountNumber == acctNo.AccountNo, cancellationToken)
                                       ?? throw new NullReferenceException(
                                           $"Account number {acctNo.AccountNo} not found");

                    jvDetails.Add(
                        new JournalVoucherDetail
                        {
                            AccountNo = acctNo.AccountNo,
                            AccountName = accountTitle.AccountName,
                            TransactionNo = existingHeaderModel.JournalVoucherHeaderNo!,
                            JournalVoucherHeaderId = existingHeaderModel.JournalVoucherHeaderId,
                            Debit = acctNo.Debit,
                            Credit = acctNo.Credit,
                        }
                    );
                }

                #endregion Details

                await _dbContext.AddRangeAsync(jvDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingHeaderModel.EditedBy,
                    $"Edited journal voucher# {existingHeaderModel.JournalVoucherHeaderNo}", "Journal Voucher");
                await _dbContext.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher edited successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit journal vouchers. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Unpost(int id, CancellationToken cancellationToken)
        {
            var jvHeader =
                await _unitOfWork.JournalVoucher.GetAsync(jv => jv.JournalVoucherHeaderId == id, cancellationToken);

            if (jvHeader == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.IsPeriodPostedAsync(Module.JournalVoucher, jvHeader.Date, cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot unpost this record because the period {jvHeader.Date:MMM yyyy} is already closed.");
                }

                jvHeader.PostedBy = null;
                jvHeader.PostedDate = null;
                jvHeader.Status = nameof(JvStatus.Pending);

                await _unitOfWork.CheckVoucher.RemoveRecords<GeneralLedgerBook>(
                    gl => gl.Reference == jvHeader.JournalVoucherHeaderNo, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Unposted journal voucher# {jvHeader.JournalVoucherHeaderNo}", "Journal Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher has been unposted.";

                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to unpost journal voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Unposted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Print), new { id });
            }
        }

        [Authorize(Roles = "Admin,AccountingManager,ManagementAccountingManager")]
        public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.JournalVoucher
                .GetAsync(cv => cv.JournalVoucherHeaderId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            if (model.Status != nameof(JvStatus.ForApproval))
            {
                TempData["error"] = "This record is not pending for approval.";
                return RedirectToAction(nameof(Print), new { id });
            }

            var isApprover =
                User.IsInRole("Admin") ||
                (User.IsInRole("AccountingManager") && model.JvType == nameof(JvType.Liquidation)) ||
                User.IsInRole("ManagementAccountingManager");

            if (!isApprover)
            {
                TempData["error"] = "You have no access to do this action.";
                return RedirectToAction(nameof(Print), new { id });
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.ApprovedBy = GetUserFullName();
                model.ApprovedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(JvStatus.Pending);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Approved journal voucher# {model.JournalVoucherHeaderNo}", "Journal Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Journal Voucher has been Approved.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to approve journal voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Approved by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNonTradeSupplierSelectList(CancellationToken cancellationToken = default)
        {
            var selectList = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
            return Json(selectList);
        }
    }
}
