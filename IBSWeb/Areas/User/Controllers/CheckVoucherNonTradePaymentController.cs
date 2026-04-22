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
using IBS.Services;
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
    [DepartmentAuthorize(
        SD.Department_Accounting,
        SD.Department_RCD,
        SD.Department_ManagementAccounting,
        SD.Department_HRAndAdminOrLegal,
        SD.Department_Finance)]
    public class CheckVoucherNonTradePaymentController: Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ApplicationDbContext _dbContext;

        private readonly ICloudStorageService _cloudStorageService;

        private readonly ILogger<CheckVoucherNonTradePaymentController> _logger;

        private readonly ISubAccountResolver _subAccountResolver;

        public CheckVoucherNonTradePaymentController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            ICloudStorageService cloudStorageService,
            ILogger<CheckVoucherNonTradePaymentController> logger,
            ISubAccountResolver subAccountResolver)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _dbContext = dbContext;
            _cloudStorageService = cloudStorageService;
            _logger = logger;
            _subAccountResolver = subAccountResolver;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private string GenerateFileNameToSave(string incomingFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(incomingFileName);
            var extension = Path.GetExtension(incomingFileName);
            return $"{fileName}-{DateTimeHelper.GetCurrentPhilippineTime():yyyyMMddHHmmss}{extension}";
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentCheckVouchers([FromForm] DataTablesParameters parameters,
            DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {
                var checkVoucherHeaders = _unitOfWork.CheckVoucher
                    .GetAllQuery(x => x.CvType == nameof(CVType.Payment));

                var totalRecords = await checkVoucherHeaders.CountAsync(cancellationToken);

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasDate = DateOnly.TryParse(searchValue, out var date);

                    checkVoucherHeaders = checkVoucherHeaders
                        .Where(s =>
                            s.CheckVoucherHeaderNo!.ToLower().Contains(searchValue) == true ||
                            s.Total.ToString().Contains(searchValue) ||
                            s.Payee!.ToLower().Contains(searchValue) == true ||
                            (hasDate && s.Date == date) ||
                            s.Reference!.ToLower().Contains(searchValue) == true ||
                            s.Status.ToLower().Contains(searchValue) ||
                            s.Particulars!.ToLower().Contains(searchValue) == true ||
                            s.CheckNo!.ToLower().Contains(searchValue) == true
                        );
                }

                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    checkVoucherHeaders = checkVoucherHeaders.Where(s => s.Date == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    checkVoucherHeaders = checkVoucherHeaders
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await checkVoucherHeaders.CountAsync(cancellationToken);

                var pagedData = await checkVoucherHeaders
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
                _logger.LogError(ex, "Failed to get check voucher payment. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Print(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            var header = await _unitOfWork.CheckVoucher
                .GetAsync(cvh => cvh.CheckVoucherHeaderId == id.Value, cancellationToken);

            if (header == null)
            {
                return NotFound();
            }

            var details = await _dbContext.CheckVoucherDetails
                .Where(cvd => cvd.CheckVoucherHeaderId == header.CheckVoucherHeaderId)
                .ToListAsync(cancellationToken);

            var viewModel = new CheckVoucherVM { Header = header, Details = details };

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview check voucher# {header.CheckVoucherHeaderNo}",
                "Check Voucher");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(viewModel);
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var cv = await _unitOfWork.CheckVoucher
                .GetAsync(x => x.CheckVoucherHeaderId == id, cancellationToken);

            if (cv == null)
            {
                return NotFound();
            }

            if (!cv.IsPrinted)
            {
                cv.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(),
                    $"Printed original copy of check voucher# {cv.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(),
                    $"Printed re-printed copy of check voucher# {cv.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var modelHeader =
                await _unitOfWork.CheckVoucher.GetAsync(cv => cv.CheckVoucherHeaderId == id, cancellationToken);

            if (modelHeader == null)
            {
                return NotFound();
            }

            var modelDetails = await _dbContext.CheckVoucherDetails
                .Where(cvd => cvd.CheckVoucherHeaderId == modelHeader.CheckVoucherHeaderId && !cvd.IsDisplayEntry)
                .ToListAsync(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CheckVoucher, modelHeader.Date, cancellationToken))
                {
                    TempData["error"] =
                        $"Cannot post this record because the period {modelHeader.Date:MMM yyyy} is already closed.";
                    return RedirectToAction(nameof(Print), new { id });
                }

                modelHeader.PostedBy = GetUserFullName();
                modelHeader.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                modelHeader.Status = modelHeader.EmployeeId == null
                    ? nameof(CheckVoucherPaymentStatus.Posted)
                    : nameof(CheckVoucherPaymentStatus.Unliquidated);

                await _unitOfWork.CheckVoucher.PostAsync(modelHeader, modelDetails, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Posted check voucher# {modelHeader.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                var updateMultipleInvoicingVoucher = await _dbContext.MultipleCheckVoucherPayments
                    .Where(p => p.CheckVoucherHeaderPaymentId == id)
                    .Include(p => p.CheckVoucherHeaderInvoice)
                    .ToListAsync(cancellationToken);

                foreach (var invoice in updateMultipleInvoicingVoucher)
                {
                    var actualPostedAmount = await _dbContext.MultipleCheckVoucherPayments
                        .Include(p => p.CheckVoucherHeaderPayment)
                        .Where(p => p.CheckVoucherHeaderInvoiceId == invoice.CheckVoucherHeaderInvoiceId &&
                                    (p.CheckVoucherHeaderPayment!.Status == nameof(CheckVoucherPaymentStatus.Posted) ||
                                     p.CheckVoucherHeaderPaymentId == id)) // Include current payment being posted
                        .SumAsync(p => p.AmountPaid, cancellationToken);

                    if (actualPostedAmount >= invoice.CheckVoucherHeaderInvoice!.InvoiceAmount)
                    {
                        invoice.CheckVoucherHeaderInvoice.IsPaid = true;
                        invoice.CheckVoucherHeaderInvoice.Status = nameof(CheckVoucherInvoiceStatus.Paid);
                    }
                    else
                    {
                        invoice.CheckVoucherHeaderInvoice.IsPaid = false;
                        invoice.CheckVoucherHeaderInvoice.Status = nameof(CheckVoucherInvoiceStatus.ForPayment);
                    }
                }

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check Voucher has been Posted.";

                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to post check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);

                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks,
            CancellationToken cancellationToken)
        {
            var existingHeaderModel = await _dbContext.CheckVoucherHeaders
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.CheckVoucherHeaderId == id, cancellationToken);

            if (existingHeaderModel == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                existingHeaderModel.CanceledBy = GetUserFullName();
                existingHeaderModel.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.Status = nameof(CheckVoucherPaymentStatus.Canceled);
                existingHeaderModel.CancellationRemarks = cancellationRemarks;

                var getCVs = await _dbContext.MultipleCheckVoucherPayments
                    .Where(cvp => cvp.CheckVoucherHeaderPaymentId == existingHeaderModel.CheckVoucherHeaderId)
                    .Include(cvp => cvp.CheckVoucherHeaderInvoice)
                    .Include(cvp => cvp.CheckVoucherHeaderPayment)
                    .ToListAsync(cancellationToken);

                foreach (var cv in getCVs)
                {
                    var existingDetails = await _dbContext.CheckVoucherDetails
                        .Where(d => d.CheckVoucherHeaderId == cv.CheckVoucherHeaderInvoiceId &&
                                    d.SubAccountId == existingHeaderModel.SupplierId)
                        .ToListAsync(cancellationToken);

                    foreach (var existingDetail in existingDetails)
                    {
                        existingDetail.AmountPaid = 0;
                    }

                    cv.CheckVoucherHeaderInvoice!.AmountPaid -= cv.AmountPaid;
                    cv.CheckVoucherHeaderInvoice.IsPaid = false;
                }

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Canceled check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message =
                        $"Check Voucher #{existingHeaderModel.CheckVoucherHeaderNo} has been cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Failed to cancel check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
        {
            var existingHeaderModel = await _dbContext.CheckVoucherHeaders
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.CheckVoucherHeaderId == id, cancellationToken);

            if (existingHeaderModel == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var getCVs = await _dbContext.MultipleCheckVoucherPayments
                    .Where(cvp => cvp.CheckVoucherHeaderPaymentId == existingHeaderModel.CheckVoucherHeaderId)
                    .Include(cvp => cvp.CheckVoucherHeaderInvoice)
                    .Include(cvp => cvp.CheckVoucherHeaderPayment)
                    .ToListAsync(cancellationToken);

                foreach (var cv in getCVs)
                {
                    var existingDetails = await _dbContext.CheckVoucherDetails
                        .Where(d => d.CheckVoucherHeaderId == cv.CheckVoucherHeaderInvoiceId &&
                                    d.SubAccountId == existingHeaderModel.SupplierId)
                        .ToListAsync(cancellationToken);

                    foreach (var existingDetail in existingDetails)
                    {
                        existingDetail.AmountPaid -= cv.AmountPaid;

                        // Ensure it doesn't go negative
                        if (existingDetail.AmountPaid < 0)
                        {
                            existingDetail.AmountPaid = 0;
                        }
                    }

                    cv.CheckVoucherHeaderInvoice!.AmountPaid -= cv.AmountPaid;
                    cv.CheckVoucherHeaderInvoice.AmountPaid = Math.Max(0, cv.CheckVoucherHeaderInvoice.AmountPaid);

                    if (cv.CheckVoucherHeaderInvoice.AmountPaid < cv.CheckVoucherHeaderInvoice.InvoiceAmount)
                    {
                        cv.CheckVoucherHeaderInvoice.IsPaid = false;
                        cv.CheckVoucherHeaderInvoice.Status = nameof(CheckVoucherInvoiceStatus.ForPayment);
                    }
                }

                existingHeaderModel.PostedBy = null;
                existingHeaderModel.VoidedBy = GetUserFullName();
                existingHeaderModel.VoidedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.Status = nameof(CheckVoucherPaymentStatus.Voided);

                await _unitOfWork.GeneralLedger.ReverseEntries(existingHeaderModel.CheckVoucherHeaderNo,
                    cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Voided check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message =
                        $"Check Voucher #{existingHeaderModel.CheckVoucherHeaderNo} has been voided successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to void check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Voided by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        public async Task<IActionResult> Unpost(int id, CancellationToken cancellationToken)
        {
            var cvHeader =
                await _unitOfWork.CheckVoucher.GetAsync(cv => cv.CheckVoucherHeaderId == id, cancellationToken);

            if (cvHeader == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CheckVoucher, cvHeader.Date, cancellationToken))
                {
                    TempData["error"] =
                        $"Cannot unpost this record because the period {cvHeader.Date:MMM yyyy} is already closed.";
                    return RedirectToAction(nameof(Print), new { id });
                }

                if (cvHeader.DcrDate != null)
                {
                    TempData["error"] =
                        "This record cannot be unposted because it already has a DCR date. Please contact Finance to resolve.";
                    return RedirectToAction(nameof(Print), new { id });
                }

                cvHeader.PostedBy = null;
                cvHeader.PostedDate = null;
                cvHeader.Status = nameof(CheckVoucherPaymentStatus.ForPosting);

                await _unitOfWork.CheckVoucher.RemoveRecords<GeneralLedgerBook>(
                    gl => gl.Reference == cvHeader.CheckVoucherHeaderNo, cancellationToken);

                var updateMultipleInvoicingVoucher = await _dbContext.MultipleCheckVoucherPayments
                    .Where(mcvp => mcvp.CheckVoucherHeaderPaymentId == id)
                    .Include(mcvp => mcvp.CheckVoucherHeaderInvoice)
                    .ToListAsync(cancellationToken);

                foreach (var invoice in updateMultipleInvoicingVoucher)
                {
                    if (invoice.CheckVoucherHeaderInvoice!.IsPaid)
                    {
                        invoice.CheckVoucherHeaderInvoice!.Status = nameof(CheckVoucherInvoiceStatus.ForPayment);
                    }
                }

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Unposted check voucher# {cvHeader.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check Voucher has been unposted.";

                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to unpost check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Unposted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Print), new { id });
            }
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            if (id == 0)
            {
                return NotFound();
            }

            try
            {
                var existingHeaderModel = await _unitOfWork.CheckVoucher
                    .GetAsync(cvh => cvh.CheckVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CheckVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }

                var existingDetailsModel = await _dbContext.CheckVoucherDetails
                    .Where(cvd => cvd.CheckVoucherHeaderId == existingHeaderModel.CheckVoucherHeaderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingDetailsModel == null)
                {
                    return NotFound();
                }

                var checkVoucher = await _dbContext.CheckVoucherDetails
                    .Where(cvd =>
                        cvd.CheckVoucherHeader!.SupplierId != null && cvd.CheckVoucherHeader.PostedBy != null &&
                        cvd.CheckVoucherHeader.CvType == nameof(CVType.Invoicing) ||
                        cvd.SubAccountId != null && cvd.CheckVoucherHeader.PostedBy != null &&
                        cvd.CheckVoucherHeader.CvType == nameof(CVType.Invoicing) &&
                        cvd.CheckVoucherHeaderId == cvd.CheckVoucherHeader.CheckVoucherHeaderId)
                    .Include(cvd => cvd.CheckVoucherHeader)
                    .OrderBy(cvd => cvd.CheckVoucherDetailId)
                    .Select(cvd => new SelectListItem
                    {
                        Value = cvd.CheckVoucherHeaderId.ToString(),
                        Text = cvd.CheckVoucherHeader!.CheckVoucherHeaderNo
                    })
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var suppliers =
                    await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);

                var bankAccounts = await _unitOfWork.GetBankAccountListById(cancellationToken);

                var getCVs = await _dbContext.MultipleCheckVoucherPayments
                    .Where(cvp => cvp.CheckVoucherHeaderPaymentId == existingHeaderModel.CheckVoucherHeaderId)
                    .Select(cvp => cvp.CheckVoucherHeaderInvoiceId)
                    .ToListAsync(cancellationToken);

                //for trim the system generated invoice reference to payment
                var particulars = existingHeaderModel.Particulars ?? "";
                var index = particulars.IndexOf("Payment for", StringComparison.Ordinal);

                CheckVoucherNonTradePaymentViewModel model = new()
                {
                    TransactionDate = existingHeaderModel.Date,
                    MultipleCvId = getCVs.ToArray(),
                    CheckVouchers = checkVoucher,
                    Total = existingHeaderModel.AmountPaid,
                    BankId = existingHeaderModel.BankId ?? 0,
                    Banks = bankAccounts,
                    CheckNo = existingHeaderModel.CheckNo!,
                    CheckDate = existingHeaderModel.CheckDate ?? default,
                    Particulars = index >= 0 ? particulars.Substring(0, index).Trim() : particulars,
                    Payee = existingHeaderModel.Payee!,
                    PayeeAddress = existingHeaderModel.Address,
                    PayeeTin = existingHeaderModel.Tin,
                    MultipleSupplierId = existingHeaderModel.SupplierId ?? existingDetailsModel.SubAccountId,
                    Suppliers = suppliers,
                    CvId = existingHeaderModel.CheckVoucherHeaderId,
                    MinDate = minDate
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to fetch cv non trade payment. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CheckVoucherNonTradePaymentViewModel viewModel, IFormFile? file,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            // Validate PaymentDetails
            if (viewModel.PaymentDetails == null || !viewModel.PaymentDetails.Any())
            {
                TempData["error"] = "Payment details are required.";
                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                return View(viewModel);
            }

            // Get the current payment being edited
            var existingPaymentAmounts = await _dbContext.MultipleCheckVoucherPayments
                .Where(p => p.CheckVoucherHeaderPaymentId == viewModel.CvId)
                .GroupBy(p => p.CheckVoucherHeaderInvoiceId)
                .Select(g => g.First())
                .ToDictionaryAsync(p => p.CheckVoucherHeaderInvoiceId, p => p.AmountPaid, cancellationToken);

            // Validate payment amounts
            var cvIds = viewModel.MultipleCvId ?? new int[0];
            var cvHeaders = await _dbContext.CheckVoucherHeaders
                .Where(cv => cvIds.Contains(cv.CheckVoucherHeaderId))
                .Select(cv => new { cv.CheckVoucherHeaderId, cv.InvoiceAmount, cv.AmountPaid })
                .ToDictionaryAsync(x => x.CheckVoucherHeaderId, cancellationToken);

            foreach (var payment in viewModel.PaymentDetails)
            {
                if (!cvHeaders.TryGetValue(payment.CVId, out var header))
                {
                    TempData["error"] = $"CV ID {payment.CVId} not found.";
                    viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                    viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                    viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                    viewModel.MinDate =
                        await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher,
                            cancellationToken);
                    return View(viewModel);
                }

                // Calculate remaining balance excluding this payment's current amount
                var currentPaymentAmount = existingPaymentAmounts.GetValueOrDefault(payment.CVId, 0m);
                var otherPayments = header.AmountPaid - currentPaymentAmount;
                var maxAllowedPayment = header.InvoiceAmount - otherPayments;

                if (payment.AmountPaid <= 0 || payment.AmountPaid > maxAllowedPayment)
                {
                    TempData["error"] =
                        $"Invalid amount for CV {payment.CVId}. Must be between 0 and {maxAllowedPayment:N4}.";
                    viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                    viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                    viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                    viewModel.MinDate =
                        await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher,
                            cancellationToken);
                    return View(viewModel);
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region--Get Check Voucher Invoicing

                var existingHeaderModel = await _dbContext.CheckVoucherHeaders
                    .Include(cv => cv.Supplier)
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(cv => cv.CheckVoucherHeaderId == viewModel.CvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var getCVs = await _dbContext.MultipleCheckVoucherPayments
                    .Where(cvp => cvp.CheckVoucherHeaderPaymentId == existingHeaderModel.CheckVoucherHeaderId)
                    .Include(cvp => cvp.CheckVoucherHeaderInvoice)
                    .Include(cvp => cvp.CheckVoucherHeaderPayment)
                    .ToListAsync(cancellationToken);

                foreach (var cv in getCVs)
                {
                    var existingDetails = await _dbContext.CheckVoucherDetails
                        .Where(d => d.CheckVoucherHeaderId == cv.CheckVoucherHeaderInvoiceId &&
                                    d.SubAccountId == existingHeaderModel.SupplierId)
                        .ToListAsync(cancellationToken);

                    foreach (var existingDetail in existingDetails)
                    {
                        existingDetail.AmountPaid = 0;
                    }
                }

                var invoicingVoucher = (await _unitOfWork.CheckVoucher
                        .GetAllAsync(cv => viewModel.MultipleCvId!.Contains(cv.CheckVoucherHeaderId),
                            cancellationToken))
                    .OrderBy(cv => cv.CheckVoucherHeaderId)
                    .ToList();

                foreach (var invoice in invoicingVoucher)
                {
                    var cv = viewModel.PaymentDetails.FirstOrDefault(c => c.CVId == invoice.CheckVoucherHeaderId);

                    if (cv == null)
                    {
                        return NotFound();
                    }

                    var getCvDetails = await _dbContext.CheckVoucherDetails
                        .Where(i => cv.CVId == i.CheckVoucherHeaderId &&
                                    i.SubAccountId == viewModel.MultipleSupplierId &&
                                    i.CheckVoucherHeader!.CvType == nameof(CVType.Invoicing))
                        .OrderBy(i => i.CheckVoucherHeaderId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (getCvDetails == null || getCvDetails.CheckVoucherHeaderId != cv.CVId)
                    {
                        continue;
                    }

                    getCvDetails.AmountPaid += cv.AmountPaid;
                }

                #endregion

                #region -- Saving the default entries --

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                #region -- Check Voucher Header --

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.PONo = invoicingVoucher.Select(i => i.PONo).FirstOrDefault();
                existingHeaderModel.SINo = invoicingVoucher.Select(i => i.SINo).FirstOrDefault();
                existingHeaderModel.SupplierId = viewModel.MultipleSupplierId;
                existingHeaderModel.Particulars =
                    $"{viewModel.Particulars}. Payment for {string.Join(",", invoicingVoucher.Select(i => i.CheckVoucherHeaderNo))}";
                existingHeaderModel.Total = viewModel.Total;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.CvType = nameof(CVType.Payment);
                existingHeaderModel.Reference =
                    string.Join(", ", invoicingVoucher.Select(inv => inv.CheckVoucherHeaderNo));
                existingHeaderModel.BankId = viewModel.BankId;
                existingHeaderModel.Payee = viewModel.Payee;
                existingHeaderModel.Address = viewModel.PayeeAddress;
                existingHeaderModel.Tin = viewModel.PayeeTin;
                existingHeaderModel.CheckNo = viewModel.CheckNo;
                existingHeaderModel.CheckDate = viewModel.CheckDate;
                existingHeaderModel.CheckAmount = viewModel.Total;
                existingHeaderModel.Total = viewModel.Total;
                existingHeaderModel.BankAccountName = bank.AccountName;
                existingHeaderModel.BankAccountNumber = bank.AccountNo;

                await _unitOfWork.SaveAsync(cancellationToken);

                #endregion -- Check Voucher Header --

                #region -- Multiple Payment Storing --

                foreach (var cv in getCVs)
                {
                    cv.CheckVoucherHeaderInvoice!.AmountPaid -= cv.AmountPaid;
                    cv.CheckVoucherHeaderInvoice.IsPaid = false;
                }

                _dbContext.RemoveRange(getCVs);

                foreach (var paymentDetail in viewModel.PaymentDetails)
                {
                    MultipleCheckVoucherPayment multipleCheckVoucherPayment = new()
                    {
                        Id = Guid.NewGuid(),
                        CheckVoucherHeaderPaymentId = existingHeaderModel.CheckVoucherHeaderId,
                        CheckVoucherHeaderInvoiceId = paymentDetail.CVId,
                        AmountPaid = paymentDetail.AmountPaid,
                    };

                    _dbContext.Add(multipleCheckVoucherPayment);
                    await _unitOfWork.SaveAsync(cancellationToken);
                }

                #region--Update invoicing voucher

                var updateMultipleInvoicingVoucher = await _dbContext.MultipleCheckVoucherPayments
                    .Where(mcvp => viewModel.MultipleCvId!.Contains(mcvp.CheckVoucherHeaderInvoiceId)
                                   && mcvp.CheckVoucherHeaderPaymentId == existingHeaderModel.CheckVoucherHeaderId)
                    .Include(mcvp => mcvp.CheckVoucherHeaderInvoice)
                    .ToListAsync(cancellationToken);

                foreach (var payment in updateMultipleInvoicingVoucher)
                {
                    payment.CheckVoucherHeaderInvoice!.AmountPaid += payment.AmountPaid;
                    if (payment.CheckVoucherHeaderInvoice?.AmountPaid >=
                        payment.CheckVoucherHeaderInvoice?.InvoiceAmount)
                    {
                        payment.CheckVoucherHeaderInvoice.IsPaid = true;
                    }
                }

                #endregion

                #endregion -- Multiple Payment Storing --

                #region -- Check Voucher Details --

                var existingDetailsModel = await _dbContext.CheckVoucherDetails
                    .Where(d => d.CheckVoucherHeaderId == existingHeaderModel.CheckVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                _dbContext.RemoveRange(existingDetailsModel);
                await _unitOfWork.SaveAsync(cancellationToken);

                var details = new List<CheckVoucherDetail>();

                for (var i = 0; i < viewModel.AccountNumber.Length; i++)
                {
                    if (viewModel.Debit[i] == 0 && viewModel.Credit[i] == 0)
                    {
                        continue;
                    }

                    SubAccountType? subAccountType;
                    int? subAccountId;
                    string? subAccountName = null;

                    if (viewModel.AccountTitle[i].Contains("Cash in Bank"))
                    {
                        subAccountType = SubAccountType.BankAccount;
                        subAccountId = viewModel.BankId;

                        var subAccountInfo = await _subAccountResolver.ResolveAsync(
                            subAccountType.Value,
                            subAccountId.Value,
                            cancellationToken
                        );

                        if (subAccountInfo != null)
                        {
                            subAccountName = subAccountInfo.Name;
                        }
                    }
                    else
                    {
                        subAccountType = SubAccountType.Supplier;
                        subAccountId = viewModel.MultipleSupplierId!;

                        var subAccountInfo = await _subAccountResolver.ResolveAsync(
                            subAccountType.Value,
                            subAccountId.Value,
                            cancellationToken
                        );

                        if (subAccountInfo != null)
                        {
                            subAccountName = subAccountInfo.Name;
                        }
                    }

                    details.Add(new CheckVoucherDetail
                    {
                        AccountNo = viewModel.AccountNumber[i],
                        AccountName = viewModel.AccountTitle[i],
                        TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                        CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                        Debit = viewModel.Debit[i],
                        Credit = viewModel.Credit[i],
                        Amount = 0,
                        SubAccountType = subAccountType,
                        SubAccountId = subAccountId,
                        SubAccountName = subAccountName,
                    });
                }

                await _dbContext.CheckVoucherDetails.AddRangeAsync(details, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                #endregion -- Check Voucher Details --

                #endregion -- Saving the default entries --

                #region -- Uploading file --

                if (file != null && file.Length > 0)
                {
                    existingHeaderModel.SupportingFileSavedFileName = GenerateFileNameToSave(file.FileName);
                    existingHeaderModel.SupportingFileSavedUrl =
                        await _cloudStorageService.UploadFileAsync(file,
                            existingHeaderModel.SupportingFileSavedFileName!);
                }

                #endregion -- Uploading file --

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Edited check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check voucher payment edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetMultipleSupplierDetails(int cvId, int suppId,
            CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Supplier
                .GetAsync(s => s.SupplierId == suppId, cancellationToken);

            var credit = await _dbContext.CheckVoucherDetails
                .Where(cvd => cvd.SubAccountId == suppId && cvd.CheckVoucherHeaderId == cvId)
                .Include(cvd => cvd.CheckVoucherHeader)
                .Select(cvd => new
                {
                    RemainingCredit = cvd.Amount - cvd.AmountPaid, cvd.CheckVoucherHeader!.Particulars
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Ensure that cv is not null before proceeding
            if (supplier == null || credit == null)
            {
                return Json(null);
            }

            return Json(new
            {
                Payee = supplier.SupplierName,
                PayeeAddress = supplier.SupplierAddress,
                PayeeTin = supplier.SupplierTin,
                credit.Particulars,
                Total = credit.RemainingCredit,
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetMultipleSupplier(int cvId, CancellationToken cancellationToken)
        {
            var cv = await _unitOfWork.CheckVoucher
                .GetAsync(c => c.CheckVoucherHeaderId == cvId, cancellationToken);

            // Ensure that cv is not null before proceeding
            if (cv == null)
            {
                return Json(null);
            }

            // Fetch suppliers whose IDs are in the supplierIds list
            var suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);

            return Json(new { SupplierList = suppliers });
        }

        [HttpGet]
        public async Task<JsonResult> MultipleSupplierDetails(int suppId, int cvId, CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Supplier
                .GetAsync(s => s.SupplierId == suppId, cancellationToken);

            var credit = await _dbContext.CheckVoucherDetails
                .Where(cvd => cvd.SubAccountId == suppId && cvd.CheckVoucherHeaderId == cvId)
                .Include(cvd => cvd.CheckVoucherHeader)
                .Select(cvd => new
                {
                    RemainingCredit = cvd.Amount - cvd.AmountPaid, cvd.CheckVoucherHeader!.Particulars
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Ensure that cv is not null before proceeding
            if (supplier == null || credit == null)
            {
                return Json(null);
            }

            return Json(new
            {
                Payee = supplier.SupplierName,
                PayeeAddress = supplier.SupplierAddress,
                PayeeTin = supplier.SupplierTin,
                credit.Particulars,
                Total = credit.RemainingCredit
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckVoucherInvoiceDetails(int? invoiceId,
            CancellationToken cancellationToken)
        {
            if (invoiceId == null)
            {
                return Json(null);
            }

            var invoice = await _unitOfWork.CheckVoucher
                .GetAsync(i => i.CheckVoucherHeaderId == invoiceId, cancellationToken);

            if (invoice == null)
            {
                return Json(null);
            }

            return Json(new
            {
                Payee = invoice.Supplier!.SupplierName,
                PayeeAddress = invoice.Supplier.SupplierAddress,
                PayeeTin = invoice.Supplier.SupplierTin,
                invoice.Particulars,
                Total = invoice.InvoiceAmount
            });
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var viewModel = new CheckVoucherNonTradePaymentViewModel();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);

            viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

            viewModel.MinDate =
                await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckVoucherNonTradePaymentViewModel viewModel, IFormFile? file,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            // Validate PaymentDetails
            if (viewModel.PaymentDetails == null || !viewModel.PaymentDetails.Any())
            {
                TempData["error"] = "Payment details are required.";
                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                return View(viewModel);
            }

            // Validate payment amounts against remaining balances
            var cvIds = viewModel.MultipleCvId ?? new int[0];
            var cvHeaders = await _dbContext.CheckVoucherHeaders
                .Where(cv => cvIds.Contains(cv.CheckVoucherHeaderId))
                .Select(cv => new { cv.CheckVoucherHeaderId, cv.InvoiceAmount, cv.AmountPaid })
                .ToDictionaryAsync(x => x.CheckVoucherHeaderId, cancellationToken);
            var cvDetailBalances = (await _dbContext.CheckVoucherDetails
                    .Where(cvd => cvIds.Contains(cvd.CheckVoucherHeaderId) &&
                                  cvd.SubAccountId == viewModel.MultipleSupplierId &&
                                  cvd.Amount > 0)
                    .Select(cvd => new { cvd.CheckVoucherHeaderId, cvd.Amount, cvd.AmountPaid })
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.CheckVoucherHeaderId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var payment in viewModel.PaymentDetails)
            {
                if (!cvDetailBalances.TryGetValue(payment.CVId, out var detail))
                {
                    TempData["error"] = $"CV Detail for CV ID {payment.CVId} not found.";
                    viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                    viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                    viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                    viewModel.MinDate =
                        await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher,
                            cancellationToken);
                    return View(viewModel);
                }

                if (!cvHeaders.TryGetValue(payment.CVId, out var header))
                {
                    TempData["error"] = $"CV ID {payment.CVId} not found.";
                    viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                    viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                    viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                    viewModel.MinDate =
                        await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher,
                            cancellationToken);
                    return View(viewModel);
                }

                var remainingBalance = detail.Amount - detail.AmountPaid;

                if (payment.AmountPaid <= 0 || payment.AmountPaid > remainingBalance)
                {
                    TempData["error"] =
                        $"Invalid amount for CV {payment.CVId}. Must be between 0 and {remainingBalance:N4}.";
                    viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
                    viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                    viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);
                    viewModel.MinDate =
                        await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher,
                            cancellationToken);
                    return View(viewModel);
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region--Get Check Voucher Invoicing

                var invoicingVoucher = (await _unitOfWork.CheckVoucher
                        .GetAllAsync(cv => viewModel.MultipleCvId!.Contains(cv.CheckVoucherHeaderId),
                            cancellationToken))
                    .OrderBy(cv => cv.CheckVoucherHeaderId)
                    .ToList();

                foreach (var invoice in invoicingVoucher)
                {
                    var cv = viewModel.PaymentDetails.FirstOrDefault(c => c.CVId == invoice.CheckVoucherHeaderId);

                    if (cv == null)
                    {
                        return NotFound();
                    }

                    var getCvDetails = await _dbContext.CheckVoucherDetails
                        .Where(i => cv.CVId == i.CheckVoucherHeaderId &&
                                    i.SubAccountId == viewModel.MultipleSupplierId &&
                                    i.CheckVoucherHeader!.CvType == nameof(CVType.Invoicing) &&
                                    i.Amount > 0m)
                        .OrderBy(i => i.CheckVoucherHeaderId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (getCvDetails == null || getCvDetails.CheckVoucherHeaderId != cv.CVId)
                    {
                        continue;
                    }

                    getCvDetails.AmountPaid += cv.AmountPaid;
                }

                #endregion

                #region -- Saving the default entries --

                #region -- Get Supplier

                var supplier = await _unitOfWork.Supplier
                    .GetAsync(po => po.SupplierId == viewModel.MultipleSupplierId, cancellationToken);

                if (supplier == null)
                {
                    return NotFound();
                }

                #endregion

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                #region -- Check Voucher Header --

                CheckVoucherHeader checkVoucherHeader = new()
                {
                    CheckVoucherHeaderNo =
                        await _unitOfWork.CheckVoucher.GenerateCodeMultiplePaymentAsync(
                            invoicingVoucher.Select(i => i.Type).FirstOrDefault() ??
                            throw new InvalidOperationException(), cancellationToken),
                    Date = viewModel.TransactionDate,
                    PONo = invoicingVoucher.Select(i => i.PONo).FirstOrDefault(),
                    SINo = invoicingVoucher.Select(i => i.SINo).FirstOrDefault(),
                    SupplierId = viewModel.MultipleSupplierId,
                    Particulars =
                        $"{viewModel.Particulars}. Payment for {string.Join(",", invoicingVoucher.Select(i => i.CheckVoucherHeaderNo))}",
                    Total = viewModel.Total,
                    CreatedBy = GetUserFullName(),
                    CvType = nameof(CVType.Payment),
                    Reference = string.Join(", ", invoicingVoucher.Select(inv => inv.CheckVoucherHeaderNo)),
                    BankId = viewModel.BankId,
                    Payee = viewModel.Payee,
                    Address = viewModel.PayeeAddress,
                    Tin = viewModel.PayeeTin,
                    CheckNo = viewModel.CheckNo,
                    CheckDate = viewModel.CheckDate,
                    CheckAmount = viewModel.Total,
                    Type = invoicingVoucher.Select(i => i.Type).First(),
                    SupplierName = supplier.SupplierName,
                    BankAccountName = bank.AccountName,
                    BankAccountNumber = bank.AccountNo,
                    TaxType = string.Empty,
                    VatType = string.Empty,
                };

                await _unitOfWork.CheckVoucher.AddAsync(checkVoucherHeader, cancellationToken);

                #endregion -- Check Voucher Header --

                #region -- Multiple Payment Storing --

                foreach (var paymentDetail in viewModel.PaymentDetails)
                {
                    MultipleCheckVoucherPayment multipleCheckVoucherPayment = new()
                    {
                        Id = Guid.NewGuid(),
                        CheckVoucherHeaderPaymentId = checkVoucherHeader.CheckVoucherHeaderId,
                        CheckVoucherHeaderInvoiceId = paymentDetail.CVId,
                        AmountPaid = paymentDetail.AmountPaid,
                    };

                    _dbContext.Add(multipleCheckVoucherPayment);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                #region--Update invoicing voucher

                var updateMultipleInvoicingVoucher = await _dbContext.MultipleCheckVoucherPayments
                    .Where(mcvp => viewModel.MultipleCvId!
                                       .Contains(mcvp.CheckVoucherHeaderInvoiceId) &&
                                   mcvp.CheckVoucherHeaderPaymentId == checkVoucherHeader.CheckVoucherHeaderId)
                    .Include(mcvp => mcvp.CheckVoucherHeaderInvoice)
                    .ToListAsync(cancellationToken);

                foreach (var payment in updateMultipleInvoicingVoucher)
                {
                    payment.CheckVoucherHeaderInvoice!.AmountPaid += payment.AmountPaid;
                    if (payment.CheckVoucherHeaderInvoice?.AmountPaid >=
                        payment.CheckVoucherHeaderInvoice?.InvoiceAmount)
                    {
                        payment.CheckVoucherHeaderInvoice.IsPaid = true;
                    }
                }

                #endregion

                #endregion -- Multiple Payment Storing --

                #region -- Check Voucher Details --

                List<CheckVoucherDetail> checkVoucherDetails = [];

                for (var i = 0; i < viewModel.AccountNumber.Length; i++)
                {
                    if (viewModel.Debit[i] == 0 && viewModel.Credit[i] == 0)
                    {
                        continue;
                    }

                    SubAccountType? subAccountType;
                    int? subAccountId;
                    string? subAccountName = null;

                    if (viewModel.AccountTitle[i].Contains("Cash in Bank"))
                    {
                        subAccountType = SubAccountType.BankAccount;
                        subAccountId = viewModel.BankId;

                        var subAccountInfo = await _subAccountResolver.ResolveAsync(
                            subAccountType.Value,
                            subAccountId.Value,
                            cancellationToken
                        );

                        if (subAccountInfo != null)
                        {
                            subAccountName = subAccountInfo.Name;
                        }
                    }
                    else
                    {
                        subAccountType = SubAccountType.Supplier;
                        subAccountId = viewModel.MultipleSupplierId!;

                        var subAccountInfo = await _subAccountResolver.ResolveAsync(
                            subAccountType.Value,
                            subAccountId.Value,
                            cancellationToken
                        );

                        if (subAccountInfo != null)
                        {
                            subAccountName = subAccountInfo.Name;
                        }
                    }

                    checkVoucherDetails.Add(new CheckVoucherDetail
                    {
                        AccountNo = viewModel.AccountNumber[i],
                        AccountName = viewModel.AccountTitle[i],
                        TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                        CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                        Debit = viewModel.Debit[i],
                        Credit = viewModel.Credit[i],
                        Amount = 0,
                        SubAccountType = subAccountType,
                        SubAccountId = subAccountId,
                        SubAccountName = subAccountName,
                    });
                }

                await _dbContext.CheckVoucherDetails.AddRangeAsync(checkVoucherDetails, cancellationToken);

                #endregion -- Check Voucher Details --

                #endregion -- Saving the default entries --

                #region -- Uploading file --

                if (file != null && file.Length > 0)
                {
                    checkVoucherHeader.SupportingFileSavedFileName = GenerateFileNameToSave(file.FileName);
                    checkVoucherHeader.SupportingFileSavedUrl =
                        await _cloudStorageService.UploadFileAsync(file,
                            checkVoucherHeader.SupportingFileSavedFileName!);
                }

                #endregion -- Uploading file --

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Created new check voucher# {checkVoucherHeader.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] =
                    $"Check voucher payment #{checkVoucherHeader.CheckVoucherHeaderNo} created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                viewModel.Suppliers = await _unitOfWork.GetNonTradeSupplierListAsyncById(cancellationToken);

                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> GetSupplierDetails(int? supplierId,
            CancellationToken cancellationToken = default)
        {
            if (supplierId == null)
            {
                return Json(null);
            }

            var supplier = await _unitOfWork.Supplier
                .GetAsync(s => s.SupplierId == supplierId, cancellationToken);

            if (supplier == null)
            {
                return Json(null);
            }

            return Json(new
            {
                Name = supplier.SupplierName,
                Address = supplier.SupplierAddress,
                TinNo = supplier.SupplierTin,
                supplier.TaxType,
                supplier.Category,
                TaxPercent = supplier.WithholdingTaxPercent,
                supplier.VatType,
                DefaultExpense = supplier.DefaultExpenseNumber,
                WithholdingTax = supplier.WithholdingTaxTitle
            });
        }

        public async Task<IActionResult> GetCVs(int supplierId, int? paymentId, CancellationToken cancellationToken)
        {
            try
            {
                var availableCVs = await _dbContext.CheckVoucherDetails
                    .Include(cvd => cvd.CheckVoucherHeader)
                    .Where(cvd => cvd.SubAccountId == supplierId &&
                                  cvd.CheckVoucherHeader!.PostedBy != null &&
                                  cvd.CheckVoucherHeader.CvType == nameof(CVType.Invoicing) &&
                                  cvd.Amount > cvd.AmountPaid) // Only show if this supplier's portion is unpaid
                    .Select(cvd => new
                    {
                        Id = cvd.CheckVoucherHeaderId,
                        CVNumber = cvd.CheckVoucherHeader!.CheckVoucherHeaderNo,
                        RemainingBalance = cvd.Amount - cvd.AmountPaid
                    })
                    .Distinct()
                    .Where(cv => cv.RemainingBalance > 0) // Only CVs with remaining balance
                    .Select(cv => new { cv.Id, cv.CVNumber })
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (paymentId != null)
                {
                    var existingPaymentCVs = await _dbContext.MultipleCheckVoucherPayments
                        .Where(m => m.CheckVoucherHeaderPaymentId == paymentId)
                        .Include(m => m.CheckVoucherHeaderInvoice)
                        .Select(m => new
                        {
                            Id = m.CheckVoucherHeaderInvoiceId,
                            CVNumber = m.CheckVoucherHeaderInvoice!.CheckVoucherHeaderNo
                        })
                        .ToListAsync(cancellationToken);

                    foreach (var cv in existingPaymentCVs)
                    {
                        if (!availableCVs.Any(a => a.Id == cv.Id))
                        {
                            availableCVs.Add(new { Id = cv.Id, CVNumber = cv.CVNumber });
                        }
                    }
                }

                if (!availableCVs.Any())
                {
                    return Json(null);
                }

                return Json(availableCVs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get check voucher. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMultipleInvoiceDetails(int[] cvId, int supplierId, int? paymentId,
            CancellationToken cancellationToken)
        {
            if (cvId.Length == 0)
            {
                return Json(null);
            }

            var invoices = await _dbContext.CheckVoucherDetails
                .Include(i => i.CheckVoucherHeader)
                .Where(i => cvId.Contains(i.CheckVoucherHeaderId) &&
                            i.SubAccountId == supplierId &&
                            i.Amount > 0)
                .ToListAsync(cancellationToken);

            // Get the first CV's particulars
            var firstParticulars = invoices.FirstOrDefault()?.CheckVoucherHeader?.Particulars ?? "";

            var journalEntries = new List<object>();
            var totalDebit = 0m;
            var cvBalances = new List<object>();

            // Deduplicate invoices at header level to avoid duplicates
            var dedupInvoices = invoices
                .GroupBy(i => i.CheckVoucherHeaderId)
                .Select(g => g.First())
                .ToList();

            // Get CV headers with invoice amounts
            var cvHeaders = await _dbContext.CheckVoucherHeaders
                .Where(cv => cvId.Contains(cv.CheckVoucherHeaderId))
                .Select(cv => new { cv.CheckVoucherHeaderId, cv.CheckVoucherHeaderNo, cv.InvoiceAmount, cv.AmountPaid })
                .ToDictionaryAsync(cv => cv.CheckVoucherHeaderId, cancellationToken);

            // If this is an edit operation, get saved payment amounts
            Dictionary<int, decimal> savedPaymentAmounts = new Dictionary<int, decimal>();
            if (paymentId.HasValue)
            {
                savedPaymentAmounts = await _dbContext.MultipleCheckVoucherPayments
                    .Where(p => p.CheckVoucherHeaderPaymentId == paymentId.Value)
                    .GroupBy(p => p.CheckVoucherHeaderInvoiceId)
                    .Select(g => g.First())
                    .ToDictionaryAsync(p => p.CheckVoucherHeaderInvoiceId, p => p.AmountPaid, cancellationToken);
            }

            // Build amounts dictionary for use in journal entries
            var amountsToUse = new Dictionary<int, decimal>();

            // Track which invoices were successfully processed
            var processedInvoices = new HashSet<int>();

            // Build CV balances list
            foreach (var invoice in dedupInvoices)
            {
                if (!cvHeaders.TryGetValue(invoice.CheckVoucherHeaderId, out var header))
                {
                    throw new InvalidOperationException(
                        $"Invoice header not found for CV ID {invoice.CheckVoucherHeaderId}. Please verify the data integrity.");
                }

                processedInvoices.Add(invoice.CheckVoucherHeaderId);

                decimal amountToDisplay;
                decimal maxBalance;

                if (paymentId.HasValue && savedPaymentAmounts.ContainsKey(invoice.CheckVoucherHeaderId))
                {
                    // Edit mode: show saved amount, max = remaining + saved
                    amountToDisplay = savedPaymentAmounts[invoice.CheckVoucherHeaderId];
                    var otherPayments = header.AmountPaid - amountToDisplay;
                    maxBalance = header.InvoiceAmount - otherPayments;
                }
                else
                {
                    // Create mode: show remaining balance
                    var remainingBalance = invoice.Amount - invoice.AmountPaid;
                    amountToDisplay = remainingBalance;
                    maxBalance = remainingBalance;
                }

                amountsToUse[invoice.CheckVoucherHeaderId] = amountToDisplay;

                cvBalances.Add(new
                {
                    CvId = invoice.CheckVoucherHeaderId,
                    CvNumber = invoice.TransactionNo,
                    Balance = amountToDisplay,
                    MaxBalance = maxBalance
                });
            }

            // Calculate total
            var displayTotal = amountsToUse.Values.Sum();

            // Group by account for journal entries
            var groupedInvoices = dedupInvoices
                .Where(i => processedInvoices.Contains(i.CheckVoucherHeaderId))
                .GroupBy(i => i.AccountNo);

            foreach (var group in groupedInvoices)
            {
                var balance = group.Sum(i => amountsToUse[i.CheckVoucherHeaderId]);
                journalEntries.Add(new
                {
                    AccountNumber = group.First().AccountNo,
                    AccountTitle = group.First().AccountName,
                    Debit = balance,
                    Credit = 0m
                });
                totalDebit += balance;
            }

            // Add Cash in Bank entry
            journalEntries.Add(new
            {
                AccountNumber = "101010100", AccountTitle = "Cash in Bank", Debit = 0m, Credit = displayTotal
            });

            var transactionDate = invoices
                .Select(x => (DateOnly?)x.CheckVoucherHeader!.Date)
                .Max();

            return Json(new
            {
                JournalEntries = journalEntries,
                TotalDebit = displayTotal,
                TotalCredit = displayTotal,
                Particulars = firstParticulars,
                CvBalances = cvBalances,
                TransactionDate = transactionDate
            });
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> CreateAdvancesToEmployee(CancellationToken cancellationToken)
        {
            var viewModel = new AdvancesToEmployeeViewModel();

            viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);

            viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

            viewModel.MinDate =
                await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdvancesToEmployee(AdvancesToEmployeeViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information provided was invalid.";
                viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region Save Record

                #region Header

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                CheckVoucherHeader checkVoucherHeader = new()
                {
                    CheckVoucherHeaderNo =
                        await _unitOfWork.CheckVoucher.GenerateCodeMultiplePaymentAsync(viewModel.DocumentType!,
                            cancellationToken),
                    Date = viewModel.TransactionDate,
                    Particulars = viewModel.Particulars,
                    PONo = [],
                    SINo = [],
                    Total = viewModel.Total,
                    CreatedBy = GetUserFullName(),
                    CvType = nameof(CVType.Payment),
                    BankId = viewModel.BankId,
                    Payee = viewModel.Payee,
                    SupplierName = viewModel.Payee,
                    Address = viewModel.PayeeAddress,
                    Tin = viewModel.PayeeTin,
                    CheckNo = viewModel.CheckNo,
                    CheckDate = viewModel.CheckDate,
                    CheckAmount = viewModel.Total,
                    Type = viewModel.DocumentType!,
                    IsAdvances = true,
                    EmployeeId = viewModel.EmployeeId,
                    BankAccountName = bank.AccountName,
                    BankAccountNumber = bank.AccountNo,
                    TaxType = string.Empty,
                    VatType = string.Empty
                };

                await _unitOfWork.CheckVoucher.AddAsync(checkVoucherHeader, cancellationToken);

                #endregion

                #region Details

                var accountTitlesDto = await _unitOfWork.CheckVoucher.GetListOfAccountTitleDto(cancellationToken);
                var advancesToOfficerTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020400") ??
                                             throw new ArgumentException($"Account title '101020400' not found.");
                var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                      throw new ArgumentException($"Account title '101010100' not found.");

                var checkVoucherDetails = new List<CheckVoucherDetail>
                {
                    new()
                    {
                        AccountNo = advancesToOfficerTitle.AccountNumber,
                        AccountName = advancesToOfficerTitle.AccountName,
                        TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                        CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                        Debit = viewModel.Total,
                        Credit = 0,
                        SubAccountType = SubAccountType.Employee,
                        SubAccountId = viewModel.EmployeeId,
                        SubAccountName = viewModel.Payee
                    },
                    new()
                    {
                        AccountNo = cashInBankTitle.AccountNumber,
                        AccountName = cashInBankTitle.AccountName,
                        TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                        CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                        Debit = 0,
                        Credit = viewModel.Total,
                        SubAccountType = SubAccountType.BankAccount,
                        SubAccountId = viewModel.BankId,
                        SubAccountName = $"{bank.AccountNo} {bank.AccountName}",
                    },
                };

                await _dbContext.AddRangeAsync(checkVoucherDetails, cancellationToken);

                #endregion

                #endregion

                #region Uploading File

                if (viewModel.SupportingFile != null && viewModel.SupportingFile.Length > 0)
                {
                    checkVoucherHeader.SupportingFileSavedFileName =
                        GenerateFileNameToSave(viewModel.SupportingFile.FileName);
                    checkVoucherHeader.SupportingFileSavedUrl =
                        await _cloudStorageService.UploadFileAsync(viewModel.SupportingFile,
                            checkVoucherHeader.SupportingFileSavedFileName!);
                }

                #endregion Uploading File

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Created new check voucher# {checkVoucherHeader.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] =
                    $"Check voucher payment #{checkVoucherHeader.CheckVoucherHeaderNo} created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create advances to employee. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                TempData["error"] = ex.Message;
                await transaction.RollbackAsync(cancellationToken);

                viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);

                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                return View(viewModel);
            }
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> EditAdvancesToEmployee(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var existingHeaderModel = await _unitOfWork.CheckVoucher
                    .GetAsync(cvh => cvh.CheckVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CheckVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }

                var employees = await _unitOfWork.GetEmployeeListById(cancellationToken);

                var bankAccounts = await _unitOfWork.GetBankAccountListById(cancellationToken);

                AdvancesToEmployeeViewModel model = new()
                {
                    CvId = existingHeaderModel.CheckVoucherHeaderId,
                    TransactionDate = existingHeaderModel.Date,
                    EmployeeId = existingHeaderModel.EmployeeId ?? 0,
                    Employees = employees,
                    Payee = existingHeaderModel.Payee!,
                    PayeeAddress = existingHeaderModel.Address,
                    PayeeTin = existingHeaderModel.Tin,
                    Total = existingHeaderModel.Total,
                    BankId = existingHeaderModel.BankId ?? 0,
                    Banks = bankAccounts,
                    CheckNo = existingHeaderModel.CheckNo!,
                    CheckDate = existingHeaderModel.CheckDate ?? default,
                    Particulars = existingHeaderModel.Particulars!,
                    MinDate = minDate
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to fetch cv non trade advances to employee. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdvancesToEmployee(AdvancesToEmployeeViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingHeaderModel = await _unitOfWork.CheckVoucher
                    .GetAsync(cv => cv.CheckVoucherHeaderId == viewModel.CvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                #region Update Record

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                #region Header

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.Particulars = viewModel.Particulars;
                existingHeaderModel.Total = viewModel.Total;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.BankId = viewModel.BankId;
                existingHeaderModel.Payee = viewModel.Payee;
                existingHeaderModel.SupplierName = viewModel.Payee;
                existingHeaderModel.Address = viewModel.PayeeAddress;
                existingHeaderModel.Tin = viewModel.PayeeTin;
                existingHeaderModel.CheckNo = viewModel.CheckNo;
                existingHeaderModel.CheckDate = viewModel.CheckDate;
                existingHeaderModel.CheckAmount = viewModel.Total;
                existingHeaderModel.BankAccountName = bank.AccountName;
                existingHeaderModel.BankAccountNumber = bank.AccountNo;

                await _unitOfWork.SaveAsync(cancellationToken);

                #endregion Header

                #region Details

                var existingDetailsModel = await _dbContext.CheckVoucherDetails
                    .Where(d => d.CheckVoucherHeaderId == existingHeaderModel.CheckVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                _dbContext.RemoveRange(existingDetailsModel);
                await _unitOfWork.SaveAsync(cancellationToken);

                var accountTitlesDto = await _unitOfWork.CheckVoucher.GetListOfAccountTitleDto(cancellationToken);
                var advancesToOfficerTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020400") ??
                                             throw new ArgumentException($"Account title '101020400' not found.");
                var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                      throw new ArgumentException($"Account title '101010100' not found.");

                var checkVoucherDetails = new List<CheckVoucherDetail>
                {
                    new()
                    {
                        AccountNo = advancesToOfficerTitle.AccountNumber,
                        AccountName = advancesToOfficerTitle.AccountName,
                        TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                        CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                        Debit = viewModel.Total,
                        Credit = 0,
                        SubAccountType = SubAccountType.Employee,
                        SubAccountId = viewModel.EmployeeId,
                        SubAccountName = viewModel.Payee
                    },
                    new()
                    {
                        AccountNo = cashInBankTitle.AccountNumber,
                        AccountName = cashInBankTitle.AccountName,
                        TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                        CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                        Debit = 0,
                        Credit = viewModel.Total,
                        SubAccountType = SubAccountType.BankAccount,
                        SubAccountId = viewModel.BankId,
                        SubAccountName = $"{bank.AccountNo} {bank.AccountName}",
                    },
                };

                await _dbContext.AddRangeAsync(checkVoucherDetails, cancellationToken);

                #endregion Details

                #endregion Update Record

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Edited check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check voucher payment edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit advances to employee. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                viewModel.Employees = await _unitOfWork.GetEmployeeListById(cancellationToken);

                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> GetEmployeeDetails(int? employeeId)
        {
            if (employeeId == null)
            {
                return Json(null);
            }

            var employee = await _unitOfWork.Employee
                .GetAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return Json(null);
            }

            return Json(new { Name = $"{employee.FirstName} {employee.LastName}", employee.Address, employee.TinNo, });
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> CreateAdvancesToSupplier(CancellationToken cancellationToken)
        {
            var viewModel = new AdvancesToSupplierViewModel();

            viewModel.Suppliers = await _unitOfWork.GetSupplierListAsyncById(cancellationToken);

            viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

            viewModel.MinDate =
                await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdvancesToSupplier(AdvancesToSupplierViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information provided was invalid.";
                viewModel.Suppliers = await _unitOfWork.GetSupplierListAsyncById(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region Save Record

                #region Header

                #region -- Get Supplier

                var supplier = await _unitOfWork.Supplier
                    .GetAsync(po => po.SupplierId == viewModel.SupplierId, cancellationToken);

                if (supplier == null)
                {
                    return NotFound();
                }

                #endregion --Retrieve Supplier

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                CheckVoucherHeader checkVoucherHeader = new()
                {
                    CheckVoucherHeaderNo =
                        await _unitOfWork.CheckVoucher.GenerateCodeMultiplePaymentAsync(viewModel.DocumentType!,
                            cancellationToken),
                    Date = viewModel.TransactionDate,
                    Particulars = viewModel.Particulars,
                    PONo = [],
                    SINo = [],
                    Total = viewModel.Total,
                    CreatedBy = GetUserFullName(),
                    CvType = nameof(CVType.Payment),
                    BankId = viewModel.BankId,
                    Payee = viewModel.Payee,
                    Address = viewModel.PayeeAddress,
                    Tin = viewModel.PayeeTin,
                    CheckNo = viewModel.CheckNo,
                    CheckDate = viewModel.CheckDate,
                    Type = viewModel.DocumentType!,
                    IsAdvances = true,
                    SupplierId = viewModel.SupplierId,
                    SupplierName = supplier.SupplierName,
                    BankAccountName = bank.AccountName,
                    BankAccountNumber = bank.AccountNo,
                    TaxType = supplier.TaxType,
                    VatType = supplier.VatType,
                };

                await _unitOfWork.CheckVoucher.AddAsync(checkVoucherHeader, cancellationToken);

                #endregion

                #region Details

                var accountTitlesDto = await _unitOfWork.CheckVoucher.GetListOfAccountTitleDto(cancellationToken);
                var advancesToSupplierTitle = accountTitlesDto.Find(c => c.AccountNumber == "101060100") ??
                                              throw new ArgumentException("Account title '101060100' not found.");
                var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                      throw new ArgumentException("Account title '101010100' not found.");
                var ewtTitle = accountTitlesDto.Find(c =>
                    c.AccountNumber == (supplier.WithholdingTaxTitle ?? string.Empty).Split(' ', 2).FirstOrDefault());

                var grossAmount = viewModel.Total;
                var netOfVat = supplier.VatType == SD.VatType_Vatable
                    ? _unitOfWork.CheckVoucher.ComputeNetOfVat(viewModel.Total)
                    : viewModel.Total;
                var ewtAmount =
                    _unitOfWork.CheckVoucher.ComputeEwtAmount(netOfVat, supplier.WithholdingTaxPercent ?? 0);
                var netOfEwtAmount = _unitOfWork.CheckVoucher.ComputeNetOfEwt(grossAmount, ewtAmount);
                checkVoucherHeader.CheckAmount = netOfEwtAmount;

                var checkVoucherDetails = new List<CheckVoucherDetail>();

                checkVoucherDetails.Add(new CheckVoucherDetail
                {
                    AccountNo = advancesToSupplierTitle.AccountNumber,
                    AccountName = advancesToSupplierTitle.AccountName,
                    TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                    CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                    Debit = grossAmount,
                    Credit = 0,
                    SubAccountType = SubAccountType.Supplier,
                    SubAccountId = viewModel.SupplierId,
                    SubAccountName = viewModel.Payee,
                });

                if (ewtTitle != null && ewtAmount > 0)
                {
                    checkVoucherDetails.Add(new CheckVoucherDetail
                    {
                        AccountNo = ewtTitle.AccountNumber,
                        AccountName = ewtTitle.AccountName,
                        TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                        CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                        Debit = 0,
                        Credit = ewtAmount
                    });
                }

                checkVoucherDetails.Add(new CheckVoucherDetail
                {
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountName = cashInBankTitle.AccountName,
                    TransactionNo = checkVoucherHeader.CheckVoucherHeaderNo,
                    CheckVoucherHeaderId = checkVoucherHeader.CheckVoucherHeaderId,
                    Debit = 0,
                    Credit = netOfEwtAmount,
                    SubAccountType = SubAccountType.BankAccount,
                    SubAccountId = viewModel.BankId,
                    SubAccountName = $"{bank.AccountNo} {bank.AccountName}",
                });

                await _dbContext.AddRangeAsync(checkVoucherDetails, cancellationToken);

                #endregion

                #endregion

                #region Uploading File

                if (viewModel.SupportingFile != null && viewModel.SupportingFile.Length > 0)
                {
                    checkVoucherHeader.SupportingFileSavedFileName =
                        GenerateFileNameToSave(viewModel.SupportingFile.FileName);
                    checkVoucherHeader.SupportingFileSavedUrl =
                        await _cloudStorageService.UploadFileAsync(viewModel.SupportingFile,
                            checkVoucherHeader.SupportingFileSavedFileName!);
                }

                #endregion Uploading File

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Created new check voucher# {checkVoucherHeader.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] =
                    $"Check voucher payment #{checkVoucherHeader.CheckVoucherHeaderNo} created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create advances to supplier. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                TempData["Error"] = ex.Message;
                await transaction.RollbackAsync(cancellationToken);

                viewModel.Suppliers = await _unitOfWork.GetSupplierListAsyncById(cancellationToken);

                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                return View(viewModel);
            }
        }

        [DepartmentAuthorize(
            SD.Department_Accounting,
            SD.Department_RCD,
            SD.Department_HRAndAdminOrLegal,
            SD.Department_ManagementAccounting)]
        [HttpGet]
        public async Task<IActionResult> EditAdvancesToSupplier(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var existingHeaderModel = await _unitOfWork.CheckVoucher
                    .GetAsync(cvh => cvh.CheckVoucherHeaderId == id, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                if (await _unitOfWork.IsPeriodPostedAsync(Module.CheckVoucher, existingHeaderModel.Date,
                        cancellationToken))
                {
                    throw new ArgumentException(
                        $"Cannot edit this record because the period {existingHeaderModel.Date:MMM yyyy} is already closed.");
                }


                var supplier =
                    await _unitOfWork.GetSupplierListAsyncById(cancellationToken);

                var bankAccounts = await _unitOfWork.GetBankAccountListById(cancellationToken);

                AdvancesToSupplierViewModel model = new()
                {
                    CvId = existingHeaderModel.CheckVoucherHeaderId,
                    TransactionDate = existingHeaderModel.Date,
                    SupplierId = existingHeaderModel.SupplierId ?? 0,
                    Suppliers = supplier,
                    Payee = existingHeaderModel.Payee!,
                    PayeeAddress = existingHeaderModel.Address,
                    PayeeTin = existingHeaderModel.Tin,
                    Total = existingHeaderModel.Total,
                    BankId = existingHeaderModel.BankId ?? 0,
                    Banks = bankAccounts,
                    CheckNo = existingHeaderModel.CheckNo!,
                    CheckDate = existingHeaderModel.CheckDate ?? default,
                    Particulars = existingHeaderModel.Particulars!,
                    MinDate = minDate
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to fetch cv non trade advances to supplier. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdvancesToSupplier(AdvancesToSupplierViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Suppliers = await _unitOfWork.GetSupplierListAsyncById(cancellationToken);
                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);
                viewModel.MinDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CheckVoucher, cancellationToken);
                TempData["warning"] = "The information provided was invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingHeaderModel = await _unitOfWork.CheckVoucher
                    .GetAsync(cv => cv.CheckVoucherHeaderId == viewModel.CvId, cancellationToken);

                if (existingHeaderModel == null)
                {
                    return NotFound();
                }

                #region Update Record

                #region Header

                #region -- Get Supplier

                var supplier = await _unitOfWork.Supplier
                    .GetAsync(po => po.SupplierId == viewModel.SupplierId, cancellationToken);

                if (supplier == null)
                {
                    return NotFound();
                }

                #endregion --Retrieve Supplier

                #region -- Get bank account

                var bank = await _unitOfWork.BankAccount
                    .GetAsync(b => b.BankAccountId == viewModel.BankId, cancellationToken);

                if (bank == null)
                {
                    return NotFound();
                }

                #endregion

                existingHeaderModel.Date = viewModel.TransactionDate;
                existingHeaderModel.Particulars = viewModel.Particulars;
                existingHeaderModel.Total = viewModel.Total;
                existingHeaderModel.EditedBy = GetUserFullName();
                existingHeaderModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingHeaderModel.BankId = viewModel.BankId;
                existingHeaderModel.Payee = viewModel.Payee;
                existingHeaderModel.Address = viewModel.PayeeAddress;
                existingHeaderModel.Tin = viewModel.PayeeTin;
                existingHeaderModel.CheckNo = viewModel.CheckNo;
                existingHeaderModel.CheckDate = viewModel.CheckDate;
                existingHeaderModel.SupplierId = viewModel.SupplierId;
                existingHeaderModel.SupplierName = supplier.SupplierName;
                existingHeaderModel.BankAccountName = bank.AccountName;
                existingHeaderModel.BankAccountNumber = bank.AccountNo;

                await _unitOfWork.SaveAsync(cancellationToken);

                #endregion Header

                #region Details

                var existingDetailsModel = await _dbContext.CheckVoucherDetails
                    .Where(d => d.CheckVoucherHeaderId == existingHeaderModel.CheckVoucherHeaderId)
                    .ToListAsync(cancellationToken);

                _dbContext.RemoveRange(existingDetailsModel);
                await _unitOfWork.SaveAsync(cancellationToken);

                var accountTitlesDto = await _unitOfWork.CheckVoucher.GetListOfAccountTitleDto(cancellationToken);
                var advancesToSupplierTitle = accountTitlesDto.Find(c => c.AccountNumber == "101060100") ??
                                              throw new ArgumentException("Account title '101060100' not found.");
                var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                      throw new ArgumentException("Account title '101010100' not found.");
                var ewtTitle = accountTitlesDto.Find(c =>
                    c.AccountNumber == (supplier.WithholdingTaxTitle ?? string.Empty).Split(' ', 2).FirstOrDefault());

                var grossAmount = viewModel.Total;
                var netOfVat = supplier.VatType == SD.VatType_Vatable
                    ? _unitOfWork.CheckVoucher.ComputeNetOfVat(viewModel.Total)
                    : viewModel.Total;
                var ewtAmount =
                    _unitOfWork.CheckVoucher.ComputeEwtAmount(netOfVat, supplier.WithholdingTaxPercent ?? 0);
                var netOfEwtAmount = _unitOfWork.CheckVoucher.ComputeNetOfEwt(grossAmount, ewtAmount);
                existingHeaderModel.CheckAmount = netOfEwtAmount;

                var checkVoucherDetails = new List<CheckVoucherDetail>();

                checkVoucherDetails.Add(new CheckVoucherDetail
                {
                    AccountNo = advancesToSupplierTitle.AccountNumber,
                    AccountName = advancesToSupplierTitle.AccountName,
                    TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                    CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                    Debit = grossAmount,
                    Credit = 0,
                    SubAccountType = SubAccountType.Supplier,
                    SubAccountId = viewModel.SupplierId,
                    SubAccountName = viewModel.Payee,
                });

                if (ewtTitle != null && ewtAmount > 0)
                {
                    checkVoucherDetails.Add(new CheckVoucherDetail
                    {
                        AccountNo = ewtTitle.AccountNumber,
                        AccountName = ewtTitle.AccountName,
                        TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                        CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                        Debit = 0,
                        Credit = ewtAmount
                    });
                }

                checkVoucherDetails.Add(new CheckVoucherDetail
                {
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountName = cashInBankTitle.AccountName,
                    TransactionNo = existingHeaderModel.CheckVoucherHeaderNo!,
                    CheckVoucherHeaderId = existingHeaderModel.CheckVoucherHeaderId,
                    Debit = 0,
                    Credit = netOfEwtAmount,
                    SubAccountType = SubAccountType.BankAccount,
                    SubAccountId = viewModel.BankId,
                    SubAccountName = $"{bank.AccountNo} {bank.AccountName}",
                });

                await _dbContext.AddRangeAsync(checkVoucherDetails, cancellationToken);

                #endregion Details

                #endregion Update Record

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Edited check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check voucher payment edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to edit advances to supplier. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                viewModel.Suppliers = await _unitOfWork.GetSupplierListAsyncById(cancellationToken);

                viewModel.Banks = await _unitOfWork.GetBankAccountListById(cancellationToken);

                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public IActionResult CheckNoIsExist(string checkNo, int? cvId)
        {
            if (cvId.HasValue)
            {
                var existingCheckNo = _unitOfWork.CheckVoucher
                    .GetAsync(cv => cv.CheckVoucherHeaderId == cvId)
                    .Result?
                    .CheckNo;

                if (checkNo == existingCheckNo)
                {
                    return Json(false);
                }
            }

            var exists = _unitOfWork.CheckVoucher
                .GetAllAsync(cv => cv.CanceledBy == null && cv.VoidedBy == null)
                .Result
                .Any(cv => cv.CheckNo == checkNo);

            return Json(exists);
        }

        public async Task<IActionResult> LiquidateAdvances(int id, DateOnly? liquidateDate,
            CancellationToken cancellationToken)
        {
            var existingHeaderModel = await _unitOfWork.CheckVoucher
                .GetAsync(x => x.CheckVoucherHeaderId == id, cancellationToken);

            if (existingHeaderModel == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                existingHeaderModel.Status = nameof(CheckVoucherPaymentStatus.Liquidated);
                existingHeaderModel.LiquidationDate = liquidateDate;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Liquidate check voucher# {existingHeaderModel.CheckVoucherHeaderNo}", "Check Voucher");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Check Voucher has been liquidated.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Failed to liquidate check voucher. Error: {ErrorMessage}, Stack: {StackTrace}. Liquidated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                TempData["error"] = $"Error: '{ex.Message}'";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
