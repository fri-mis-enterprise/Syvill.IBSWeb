using System.Diagnostics;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using CsvHelper;
using Humanizer;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;
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
using OfficeOpenXml;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_Finance, SD.Department_RCD)]
    public class CollectionReceiptController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CollectionReceiptController> _logger;

        private readonly ICloudStorageService _cloudStorageService;

        public CollectionReceiptController(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<CollectionReceiptController> logger,
            ICloudStorageService cloudStorageService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudStorageService = cloudStorageService;
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

        private string GenerateFileNameToSave(string incomingFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(incomingFileName);
            var extension = Path.GetExtension(incomingFileName);
            return $"{fileName}-{DateTimeHelper.GetCurrentPhilippineTime():yyyyMMddHHmmss}{extension}";
        }

        public async Task<IActionResult> Index(string? view, CancellationToken cancellationToken)
        {
            ViewBag.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            if (view != nameof(DynamicView.CollectionReceipt))
            {
                return View();
            }

            return View("ExportIndex");
        }

        public async Task<IActionResult> ServiceInvoiceIndex()
        {
            ViewBag.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCollectionReceipts([FromForm] DataTablesParameters parameters, DateOnly filterDate, string invoiceType, CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                if (companyClaims == null)
                {
                    return BadRequest();
                }

                var collectionReceipts = _unitOfWork.FilprideCollectionReceipt
                    .GetAllQuery(c => c.Company == companyClaims);

                var totalRecords = await collectionReceipts.CountAsync(cancellationToken);

                switch (invoiceType)
                {
                    case "Sales":
                        collectionReceipts = collectionReceipts
                            .Where(s => s.SalesInvoiceId != null || s.MultipleSIId != null);
                        break;

                    case "Service":
                        collectionReceipts = collectionReceipts
                            .Where(s => s.ServiceInvoiceId != null);
                        break;
                }

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasTransactionDate = DateOnly.TryParse(searchValue, out var transactionDate);

                    collectionReceipts = collectionReceipts
                        .Where(s =>
                            s.CollectionReceiptNo!.ToLower().Contains(searchValue) ||
                            s.Customer!.CustomerName.ToLower().Contains(searchValue) ||
                            s.ReceiptDetails!.Any(d =>
                                d.InvoiceNo.ToLower().Contains(searchValue)) ||
                            (hasTransactionDate && s.TransactionDate == transactionDate) ||
                            s.CreatedBy!.ToLower().Contains(searchValue) ||
                            s.Status.ToLower().Contains(searchValue)
                            );
                }
                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    collectionReceipts = collectionReceipts.Where(s => s.TransactionDate == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    collectionReceipts = collectionReceipts
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await collectionReceipts.CountAsync(cancellationToken);

                var pagedData = await collectionReceipts
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
                    .Select(c => new
                    {
                        c.CollectionReceiptId,
                        c.CollectionReceiptNo,
                        c.TransactionDate,
                        Invoices = c.ReceiptDetails!
                            .Select(a => a.InvoiceNo)
                            .ToList(),
                        c.Customer!.CustomerName,
                        c.Total,
                        c.CreatedBy,
                        c.Status,
                        c.VoidedBy,
                        c.PostedBy,
                        c.CanceledBy,
                        c.MultipleSIId,
                        c.DepositedDate,
                    })
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
                _logger.LogError(ex, "Failed to get collection receipts. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> SingleCollectionCreateForSales(CancellationToken cancellationToken)
        {
            var viewModel = new CollectionReceiptSingleSiViewModel();
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByAccountTitle(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            return View(viewModel);
        }

        public async Task<IActionResult> GetBanks(CancellationToken cancellationToken = default)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            return Json(await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> Deposit(int id, int bankId, DateOnly depositDate, CancellationToken cancellationToken)
        {
            var bank = await _unitOfWork.FilprideBankAccount
                .GetAsync(b => b.BankAccountId == bankId, cancellationToken);

            if (bank == null)
            {
                return NotFound();
            }

            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.DepositedDate = depositDate;
                model.BankId = bank.BankAccountId;
                model.BankAccountName = bank.AccountName;
                model.BankAccountNumber = bank.AccountNo;
                model.Status = nameof(CollectionReceiptStatus.Deposited);

                model.ReceiptDetails = await _dbContext.CollectionReceiptDetails
                    .Where(rd => rd.CollectionReceiptId == model.CollectionReceiptId)
                    .ToListAsync(cancellationToken);

                await _unitOfWork.FilprideCollectionReceipt.DepositAsync(model, cancellationToken);

                foreach (var receipt in model.ReceiptDetails!)
                {
                    var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                                           .GetAsync(x => x.SalesInvoiceNo == receipt.InvoiceNo
                                                          && x.Company == model.Company, cancellationToken);

                    if (salesInvoice == null)
                    {
                        continue;
                    }
                    var getHolidays = await DateTimeHelper.GetNonWorkingDays(salesInvoice.DueDate, depositDate, "PH");
                    var daysDelayed = depositDate.DayNumber - salesInvoice.DueDate.DayNumber - getHolidays.Count;

                    if (daysDelayed <= 0 || salesInvoice.DeliveryReceipt == null || salesInvoice.DeliveryReceipt?.CommissionAmount <= 0)
                    {
                        continue;
                    }

                    var dr = salesInvoice.DeliveryReceipt!;

                    //Formula: Commission Amount x 3% x Days Delayed / 360
                    var costOfMoney = dr.CommissionAmount * .03m * daysDelayed / 360m;

                    await _unitOfWork.FilprideCollectionReceipt.ApplyCostOfMoney(dr, costOfMoney,
                        GetUserFullName(), depositDate, cancellationToken);
                }

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Record deposit date of collection receipt#{model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt deposited date has been recorded successfully.";

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to record deposit date. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SingleCollectionCreateForSales(CollectionReceiptSingleSiViewModel viewModel, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);
            viewModel.SalesInvoices = (await _unitOfWork.FilprideSalesInvoice.GetAllAsync(si => si.Company == companyClaims
                    && si.Balance > 0
                    && si.CustomerId == viewModel.CustomerId
                    && si.PostedBy != null, cancellationToken))
                .OrderBy(s => s.SalesInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.SalesInvoiceId.ToString(),
                    Text = s.SalesInvoiceNo
                })
                .ToList();
            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);
            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["warning"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                var existingSalesInvoice = await _unitOfWork.FilprideSalesInvoice
                    .GetAsync(si => si.SalesInvoiceId == viewModel.SalesInvoiceId, cancellationToken);

                if (existingSalesInvoice == null)
                {
                    return NotFound();
                }

                var model = new CollectionReceipt
                {
                    CollectionReceiptNo = await _unitOfWork.FilprideCollectionReceipt
                        .GenerateCodeAsync(companyClaims, existingSalesInvoice.Type, cancellationToken),
                    SalesInvoiceId = existingSalesInvoice.SalesInvoiceId,
                    SINo = existingSalesInvoice.SalesInvoiceNo,
                    CustomerId = viewModel.CustomerId,
                    TransactionDate = viewModel.TransactionDate,
                    ReferenceNo = viewModel.ReferenceNo,
                    Remarks = viewModel.Remarks,
                    CashAmount = viewModel.CashAmount,
                    CheckDate = viewModel.CheckDate,
                    CheckNo = viewModel.CheckNo,
                    CheckBank = viewModel.CheckBank,
                    CheckBranch = viewModel.CheckBranch,
                    CheckAmount = viewModel.CheckAmount,
                    ManagersCheckDate = viewModel.ManagersCheckDate,
                    ManagersCheckNo = viewModel.ManagersCheckNo,
                    ManagersCheckBank = viewModel.ManagersCheckBank,
                    ManagersCheckBranch = viewModel.ManagersCheckBranch,
                    ManagersCheckAmount = viewModel.ManagersCheckAmount,
                    EWT = viewModel.EWT,
                    WVAT = viewModel.WVAT,
                    Total = total,
                    CreatedBy = GetUserFullName(),
                    Company = companyClaims,
                    Type = existingSalesInvoice.Type,
                    BatchNumber = viewModel.BatchNumber
                };

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    model.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    model.F2306FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, model.F2306FileName!);
                    model.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    model.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    model.F2307FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, model.F2307FileName!);
                    model.IsCertificateUpload = true;
                }

                await _unitOfWork.FilprideCollectionReceipt.AddAsync(model, cancellationToken);

                var details = new CollectionReceiptDetail
                {
                    CollectionReceiptId = model.CollectionReceiptId,
                    CollectionReceiptNo = model.CollectionReceiptNo,
                    InvoiceDate = DateOnly.FromDateTime(existingSalesInvoice.CreatedDate),
                    InvoiceNo = existingSalesInvoice.SalesInvoiceNo!,
                    Amount = model.Total
                };

                await _dbContext.CollectionReceiptDetails.AddAsync(details, cancellationToken);

                #endregion --Saving default value

                await _unitOfWork.FilprideCollectionReceipt.UpdateInvoice(model.SalesInvoice!.SalesInvoiceId, model.Total, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!,
                    $"Create new collection receipt# {model.CollectionReceiptNo}", "Collection Receipt",
                    model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                TempData["success"] = $"Collection receipt #{model.CollectionReceiptNo} created successfully.";
                await transaction.CommitAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create sales invoice single collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> MultipleCollectionCreateForSales(CancellationToken cancellationToken)
        {
            var viewModel = new CollectionReceiptMultipleSiViewModel();
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultipleCollectionCreateForSales(CollectionReceiptMultipleSiViewModel viewModel, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            viewModel.SalesInvoices = (await _unitOfWork.FilprideSalesInvoice.GetAllAsync(si => si.Company == companyClaims
                    && si.Balance > 0
                    && si.CustomerId == viewModel.CustomerId
                    && si.PostedBy != null, cancellationToken))
                .OrderBy(s => s.SalesInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.SalesInvoiceId.ToString(),
                    Text = s.SalesInvoiceNo
                })
                .ToList();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["warning"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                var model = new CollectionReceipt
                {
                    TransactionDate = viewModel.TransactionDate,
                    CustomerId = viewModel.CustomerId,
                    ReferenceNo = viewModel.ReferenceNo,
                    Remarks = viewModel.Remarks,
                    CashAmount = viewModel.CashAmount,
                    CheckAmount = viewModel.CheckAmount,
                    CheckNo = viewModel.CheckNo,
                    CheckBranch = viewModel.CheckBranch,
                    CheckDate = viewModel.CheckDate,
                    CheckBank = viewModel.CheckBank,
                    ManagersCheckDate = viewModel.ManagersCheckDate,
                    ManagersCheckNo = viewModel.ManagersCheckNo,
                    ManagersCheckBank = viewModel.ManagersCheckBank,
                    ManagersCheckBranch = viewModel.ManagersCheckBranch,
                    ManagersCheckAmount = viewModel.ManagersCheckAmount,
                    EWT = viewModel.EWT,
                    WVAT = viewModel.WVAT,
                    Total = total,
                    CreatedBy = GetUserFullName(),
                    Company = companyClaims,
                    MultipleSIId = viewModel.MultipleSIId,
                    SIMultipleAmount = viewModel.SIMultipleAmount,
                    BatchNumber = viewModel.BatchNumber
                };

                model.MultipleSI = new string[model.MultipleSIId.Length];
                model.MultipleTransactionDate = new DateOnly[model.MultipleSIId.Length];

                await _unitOfWork.FilprideCollectionReceipt.AddAsync(model, cancellationToken);

                var details = new List<CollectionReceiptDetail>();

                for (var i = 0; i < viewModel.MultipleSIId.Length; i++)
                {
                    var siId = viewModel.MultipleSIId[i];
                    var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                        .GetAsync(si => si.SalesInvoiceId == siId, cancellationToken);

                    if (salesInvoice == null)
                    {
                        throw new InvalidOperationException("Sales Invoice not found");
                    }

                    model.MultipleSI[i] = salesInvoice.SalesInvoiceNo!;
                    model.MultipleTransactionDate[i] = salesInvoice.TransactionDate;

                    if (model.Type == null)
                    {
                        model.Type = salesInvoice.Type;

                        model.CollectionReceiptNo = await _unitOfWork.FilprideCollectionReceipt
                            .GenerateCodeAsync(companyClaims, model.Type!, cancellationToken);
                    }

                    details.Add(new CollectionReceiptDetail
                    {
                        CollectionReceiptId = model.CollectionReceiptId,
                        CollectionReceiptNo = model.CollectionReceiptNo!,
                        InvoiceDate = DateOnly.FromDateTime(salesInvoice.CreatedDate),
                        InvoiceNo = salesInvoice.SalesInvoiceNo!,
                        Amount = viewModel.SIMultipleAmount[i],
                    });
                }

                await _dbContext.CollectionReceiptDetails.AddRangeAsync(details, cancellationToken);

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    model.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    model.F2306FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, model.F2306FileName!);
                    model.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    model.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    model.F2307FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, model.F2307FileName!);
                    model.IsCertificateUpload = true;
                }

                #endregion --Saving default value

                await _unitOfWork.FilprideCollectionReceipt.UpdateMultipleInvoice(model.MultipleSI!, model.SIMultipleAmount, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!,
                    $"Create new collection receipt# {model.CollectionReceiptNo}", "Collection Receipt",
                    model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                TempData["success"] = $"Collection receipt #{model.CollectionReceiptNo} created successfully.";
                await transaction.CommitAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create sales invoice multiple collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> MultipleCollectionEdit(int? id, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (id == null)
            {
                return NotFound();
            }
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var minDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            if (await _unitOfWork.IsPeriodPostedAsync(Module.CollectionReceipt, existingModel.TransactionDate, cancellationToken))
            {
                throw new ArgumentException($"Cannot edit this record because the period {existingModel.TransactionDate:MMM yyyy} is already closed.");
            }

            var listOfDetails = await _dbContext.CollectionReceiptDetails
                .Where(x => x.CollectionReceiptId == id).ToListAsync(cancellationToken);

            var crPayments = new List<InvoicePayment>();

            foreach (var detail in listOfDetails)
            {
                var crPayment = new InvoicePayment
                {
                    InvoiceId = (await _dbContext.SalesInvoices
                            .Where(si => si.SalesInvoiceNo == detail.InvoiceNo).FirstOrDefaultAsync(cancellationToken))!
                        .SalesInvoiceId,
                    InvoiceNumber = detail.InvoiceNo,
                    PaymentAmount = detail.Amount
                };
                crPayments.Add(crPayment);
            }

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptNo == existingModel.CollectionReceiptNo)
                .Select(crd => crd.InvoiceNo)
                .ToListAsync(cancellationToken);

            var viewModel = new CollectionReceiptMultipleSiViewModel
            {
                CollectionReceiptId = existingModel.CollectionReceiptId,
                CustomerId = existingModel.CustomerId,
                Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken),
                TransactionDate = existingModel.TransactionDate,
                ReferenceNo = existingModel.ReferenceNo,
                Remarks = existingModel.Remarks,
                MultipleSIId = existingModel.MultipleSIId!,
                SalesInvoices = (await _unitOfWork.FilprideSalesInvoice
                        .GetAllAsync(si =>
                                si.Company == companyClaims &&
                                (
                                    (si.Balance > 0 || invoicesPaid.Contains(si.SalesInvoiceNo!)) &&
                                    si.CustomerId == existingModel.CustomerId &&
                                    si.PostedBy != null
                                ),
                            cancellationToken))
                    .OrderBy(s => s.SalesInvoiceId)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SalesInvoiceId.ToString(),
                        Text = s.SalesInvoiceNo
                    })
                    .ToList(),
                CashAmount = existingModel.CashAmount,
                CheckBranch = existingModel.CheckBranch,
                CheckNo = existingModel.CheckNo,
                CheckDate = existingModel.CheckDate,
                CheckAmount = existingModel.CheckAmount,
                CheckBank = existingModel.CheckBank,
                ManagersCheckDate = existingModel.ManagersCheckDate,
                ManagersCheckNo = existingModel.ManagersCheckNo,
                ManagersCheckBank = existingModel.ManagersCheckBank,
                ManagersCheckBranch = existingModel.ManagersCheckBranch,
                ManagersCheckAmount = existingModel.ManagersCheckAmount,
                BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken),
                EWT = existingModel.EWT,
                WVAT = existingModel.WVAT,
                HasAlready2306 = existingModel.F2306FilePath != null,
                HasAlready2307 = existingModel.F2307FilePath != null,
                ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken),
                SIMultipleAmount = existingModel.SIMultipleAmount!,
                InvoicePayments = crPayments,
                MinDate = minDate,
                BatchNumber = existingModel.BatchNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultipleCollectionEdit(CollectionReceiptMultipleSiViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == viewModel.CollectionReceiptId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptNo == existingModel.CollectionReceiptNo)
                .Select(crd => crd.InvoiceNo)
                .ToListAsync(cancellationToken);

            viewModel.SalesInvoices = (await _unitOfWork.FilprideSalesInvoice.GetAllAsync(si => si.Company == companyClaims
                    && (si.Balance > 0 || invoicesPaid.Contains(si.SalesInvoiceNo!))
                    && si.CustomerId == existingModel.CustomerId
                    && si.PostedBy != null, cancellationToken))
                .OrderBy(s => s.SalesInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.SalesInvoiceId.ToString(),
                    Text = s.SalesInvoiceNo
                })
                .ToList();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["error"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                // get existing details
                var listOfDetails = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .ToListAsync(cancellationToken);

                foreach (var detail in listOfDetails)
                {
                    // based on details, revert the calculation done to sales invoices
                    await _unitOfWork.FilprideCollectionReceipt.UndoSalesInvoiceChanges(detail, cancellationToken);
                }

                // delete all details
                await _dbContext.CollectionReceiptDetails
                    .Where(x => x.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .ExecuteDeleteAsync(cancellationToken);

                var details = new List<CollectionReceiptDetail>();

                existingModel.CustomerId = viewModel.CustomerId;
                existingModel.TransactionDate = viewModel.TransactionDate;
                existingModel.ReferenceNo = viewModel.ReferenceNo;
                existingModel.Remarks = viewModel.Remarks;
                existingModel.CashAmount = viewModel.CashAmount;
                existingModel.CheckAmount = viewModel.CheckAmount;
                existingModel.CheckNo = viewModel.CheckNo;
                existingModel.CheckBranch = viewModel.CheckBranch;
                existingModel.CheckDate = viewModel.CheckDate;
                existingModel.CheckBank = viewModel.CheckBank;
                existingModel.ManagersCheckDate = viewModel.ManagersCheckDate;
                existingModel.ManagersCheckNo = viewModel.ManagersCheckNo;
                existingModel.ManagersCheckBank = viewModel.ManagersCheckBank;
                existingModel.ManagersCheckBranch = viewModel.ManagersCheckBranch;
                existingModel.ManagersCheckAmount = viewModel.ManagersCheckAmount;
                existingModel.EWT = viewModel.EWT;
                existingModel.WVAT = viewModel.WVAT;
                existingModel.Total = total;
                existingModel.MultipleSIId = new int[viewModel.MultipleSIId.Length];
                existingModel.MultipleSI = new string[viewModel.MultipleSIId.Length];
                existingModel.SIMultipleAmount = new decimal[viewModel.MultipleSIId.Length];
                existingModel.MultipleTransactionDate = new DateOnly[viewModel.MultipleSIId.Length];
                existingModel.BatchNumber = viewModel.BatchNumber;

                // looping all the new SI
                for (var i = 0; i < viewModel.MultipleSIId.Length; i++)
                {
                    var siId = viewModel.MultipleSIId[i];
                    var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                        .GetAsync(si => si.SalesInvoiceId == siId, cancellationToken);

                    if (salesInvoice == null)
                    {
                        throw new InvalidOperationException("Sales Invoice not found");
                    }

                    existingModel.MultipleSIId[i] = viewModel.MultipleSIId[i];
                    existingModel.MultipleSI[i] = salesInvoice.SalesInvoiceNo!;
                    existingModel.MultipleTransactionDate[i] = salesInvoice.TransactionDate;
                    existingModel.SIMultipleAmount[i] = viewModel.SIMultipleAmount[i];

                    details.Add(new CollectionReceiptDetail
                    {
                        CollectionReceiptId = existingModel.CollectionReceiptId,
                        CollectionReceiptNo = existingModel.CollectionReceiptNo!,
                        InvoiceDate = salesInvoice.TransactionDate,
                        InvoiceNo = salesInvoice.SalesInvoiceNo!,
                        Amount = existingModel.SIMultipleAmount[i],
                    });
                }

                await _dbContext.CollectionReceiptDetails.AddRangeAsync(details, cancellationToken);

                await _unitOfWork.FilprideCollectionReceipt.UpdateMultipleInvoice(existingModel.MultipleSI!, existingModel.SIMultipleAmount!, cancellationToken);

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    existingModel.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    existingModel.F2306FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, existingModel.F2306FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    existingModel.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    existingModel.F2307FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, existingModel.F2307FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                #endregion --Saving default value

                if (!_dbContext.ChangeTracker.HasChanges())
                {
                    TempData["warning"] = "No data changes!";
                    return View(viewModel);
                }

                existingModel.EditedBy = GetUserFullName();
                existingModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update sales invoice multiple collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> CreateForService(CancellationToken cancellationToken)
        {
            var viewModel = new CollectionReceiptServiceViewModel();
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);
            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);
            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForService(CollectionReceiptServiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);
            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.ServiceInvoices = (await _unitOfWork.FilprideServiceInvoice
                .GetAllAsync(si => si.Company == companyClaims
                                   && si.Balance > 0
                                   && si.CustomerId == viewModel.CustomerId
                                   && si.PostedBy != null, cancellationToken))
                .OrderBy(si => si.ServiceInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceInvoiceId.ToString(),
                    Text = s.ServiceInvoiceNo
                })
                .ToList();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);
            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["warning"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                var existingServiceInvoice = await _dbContext.ServiceInvoices
                    .FirstOrDefaultAsync(si => si.ServiceInvoiceId == viewModel.ServiceInvoiceId,
                        cancellationToken);

                if (existingServiceInvoice == null)
                {
                    return NotFound();
                }

                var model = new CollectionReceipt
                {
                    CollectionReceiptNo = await _unitOfWork.FilprideCollectionReceipt
                        .GenerateCodeAsync(companyClaims, existingServiceInvoice.Type, cancellationToken),
                    ServiceInvoiceId = existingServiceInvoice.ServiceInvoiceId,
                    SVNo = existingServiceInvoice.ServiceInvoiceNo,
                    CustomerId = viewModel.CustomerId,
                    TransactionDate = viewModel.TransactionDate,
                    ReferenceNo = viewModel.ReferenceNo,
                    Remarks = viewModel.Remarks,
                    CashAmount = viewModel.CashAmount,
                    CheckNo = viewModel.CheckNo,
                    CheckBranch = viewModel.CheckBranch,
                    CheckDate = viewModel.CheckDate,
                    CheckAmount = viewModel.CheckAmount,
                    CheckBank = viewModel.CheckBank,
                    ManagersCheckDate = viewModel.ManagersCheckDate,
                    ManagersCheckNo = viewModel.ManagersCheckNo,
                    ManagersCheckBank = viewModel.ManagersCheckBank,
                    ManagersCheckBranch = viewModel.ManagersCheckBranch,
                    ManagersCheckAmount = viewModel.ManagersCheckAmount,
                    EWT = viewModel.EWT,
                    WVAT = viewModel.WVAT,
                    Total = total,
                    CreatedBy = GetUserFullName(),
                    Company = companyClaims,
                    Type = existingServiceInvoice.Type,
                    BatchNumber = viewModel.BatchNumber
                };

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    model.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    model.F2306FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, model.F2306FileName!);
                    model.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    model.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    model.F2307FilePath =
                        await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, model.F2307FileName!);
                    model.IsCertificateUpload = true;
                }

                await _unitOfWork.FilprideCollectionReceipt.AddAsync(model, cancellationToken);

                var details = new CollectionReceiptDetail
                {
                    CollectionReceiptId = model.CollectionReceiptId,
                    CollectionReceiptNo = model.CollectionReceiptNo,
                    InvoiceDate = DateOnly.FromDateTime(existingServiceInvoice.CreatedDate),
                    InvoiceNo = existingServiceInvoice.ServiceInvoiceNo,
                    Amount = model.Total
                };

                await _dbContext.CollectionReceiptDetails.AddAsync(details, cancellationToken);

                await _unitOfWork.FilprideCollectionReceipt.UpdateSV(model.ServiceInvoice!.ServiceInvoiceId, model.Total, 0, cancellationToken);

                #endregion --Saving default value

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!,
                    $"Create new collection receipt# {model.CollectionReceiptNo}", "Collection Receipt",
                    model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Collection receipt #{model.CollectionReceiptNo} created successfully.";
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create service invoice collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Print(int id, CancellationToken cancellationToken)
        {
            var cr = await _unitOfWork.FilprideCollectionReceipt.GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (cr == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt", companyClaims!);
            await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(cr);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesInvoices(int customerNo, int? crId, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            List<SalesInvoice> invoices;

            if (crId != null)
            {
                var invoicesPaid = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == crId)
                    .ToListAsync(cancellationToken);

                var invoiceNo = invoicesPaid
                    .Select(crd => crd.InvoiceNo);

                invoices = (await _unitOfWork.FilprideSalesInvoice
                        .GetAllAsync(si =>
                                si.Company == companyClaims &&
                                (
                                    (si.Balance > 0 || invoiceNo.Contains(si.SalesInvoiceNo!)) &&
                                    si.CustomerId == customerNo &&
                                    si.PostedBy != null
                                ),
                            cancellationToken))
                    .OrderBy(si => si.SalesInvoiceId)
                    .ToList();
            }
            else
            {
                invoices = (await _unitOfWork.FilprideSalesInvoice
                        .GetAllAsync(si => si.Company == companyClaims
                                           && si.Balance > 0
                                           && si.CustomerId == customerNo
                                           && si.PostedBy != null, cancellationToken))
                    .OrderBy(si => si.SalesInvoiceId)
                    .ToList();
            }

            var invoiceList = invoices.Select(si => new SelectListItem
            {
                Value = si.SalesInvoiceId.ToString(),   // Replace with your actual ID property
                Text = si.SalesInvoiceNo              // Replace with your actual property for display text
            }).ToList();

            return Json(invoiceList);
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceInvoices(int customerNo, int? crId, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            List<ServiceInvoice> invoices;

            if (crId != null)
            {
                var invoicesPaid = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == crId)
                    .ToListAsync(cancellationToken);

                var invoiceNo = invoicesPaid
                    .Select(crd => crd.InvoiceNo);

                invoices = (await _unitOfWork.FilprideServiceInvoice
                        .GetAllAsync(si =>
                                si.Company == companyClaims &&
                                (
                                    (si.Balance > 0 || invoiceNo.Contains(si.ServiceInvoiceNo!)) &&
                                    si.CustomerId == customerNo &&
                                    si.PostedBy != null
                                ),
                            cancellationToken))
                    .OrderBy(si => si.ServiceInvoiceId)
                    .ToList();
            }
            else
            {
                invoices = (await _unitOfWork.FilprideServiceInvoice
                        .GetAllAsync(si => si.Company == companyClaims
                                           && si.CustomerId == customerNo
                                           && si.Balance > 0
                                           && si.PostedBy != null, cancellationToken))
                    .OrderBy(si => si.ServiceInvoiceId)
                    .ToList();
            }

            var invoiceList = invoices.Select(si => new SelectListItem
            {
                Value = si.ServiceInvoiceId.ToString(),   // Replace with your actual ID property
                Text = si.ServiceInvoiceNo              // Replace with your actual property for display text
            }).ToList();

            return Json(invoiceList);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(int invoiceNo, bool isSales, bool isServices, int? crId, CancellationToken cancellationToken)
        {
            if (isSales && !isServices)
            {
                var si = await _unitOfWork.FilprideSalesInvoice
                    .GetAsync(s => s.SalesInvoiceId == invoiceNo, cancellationToken);

                if (si == null)
                {
                    return NotFound();
                }

                var vatType = si.CustomerOrderSlip?.VatType ?? si.Customer!.VatType;
                var hasEwt = si.CustomerOrderSlip?.HasEWT ?? si.Customer!.WithHoldingTax;
                var hasWvat = si.CustomerOrderSlip?.HasWVAT ?? si.Customer!.WithHoldingVat;

                var netDiscount = si.Amount - si.Discount;
                var netOfVatAmount = vatType == SD.VatType_Vatable
                    ? _unitOfWork.FilprideServiceInvoice.ComputeNetOfVat(netDiscount)
                    : netDiscount;
                var withHoldingTaxAmount = hasEwt
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m)
                    : 0;
                var withHoldingVatAmount = hasWvat
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m)
                    : 0;
                var balance = si.Balance;
                var amountPaid = si.AmountPaid;

                // it means it is in edit
                if (crId != null)
                {
                    // get the current amount of this cr
                    var collectionReceiptHeader = await _unitOfWork.FilprideCollectionReceipt
                        .GetAsync(cr => cr.CollectionReceiptId == crId, cancellationToken);
                    if (collectionReceiptHeader == null)
                    {
                        return NotFound();
                    }

                    // retain the fresh value, see if the selected cr is the one used to pay this si
                    if (collectionReceiptHeader.SalesInvoiceId == si.SalesInvoiceId)
                    {
                        amountPaid -= collectionReceiptHeader.Total;
                        balance += collectionReceiptHeader.Total;
                    }
                }

                return Json(new
                {
                    Amount = netDiscount.ToString(SD.Two_Decimal_Format),
                    AmountPaid = amountPaid.ToString(SD.Two_Decimal_Format),
                    Balance = balance.ToString(SD.Two_Decimal_Format),
                    Ewt = withHoldingTaxAmount.ToString(SD.Two_Decimal_Format),
                    Wvat = withHoldingVatAmount.ToString(SD.Two_Decimal_Format),
                    Total = (netDiscount - (withHoldingTaxAmount + withHoldingVatAmount)).ToString(SD.Two_Decimal_Format)
                });
            }

            if (isServices && !isSales)
            {
                var sv = await _unitOfWork.FilprideServiceInvoice
                    .GetAsync(s => s.ServiceInvoiceId == invoiceNo, cancellationToken);

                if (sv == null)
                {
                    return NotFound();
                }

                var netOfVatAmount = sv.VatType == SD.VatType_Vatable
                    ? _unitOfWork.FilprideServiceInvoice.ComputeNetOfVat(sv.Total) - sv.Discount
                    : sv.Total - sv.Discount;
                var withHoldingTaxAmount = sv.HasEwt
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m)
                    : 0;
                var withHoldingVatAmount = sv.HasWvat
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m)
                    : 0;
                var balance = sv.Balance;
                var amountPaid = sv.AmountPaid;

                // it means it is in edit
                if (crId != null)
                {
                    // get the current amount of this cr
                    var collectionReceiptHeader = await _unitOfWork.FilprideCollectionReceipt
                        .GetAsync(cr => cr.CollectionReceiptId == crId, cancellationToken);
                    if (collectionReceiptHeader == null)
                    {
                        return NotFound();
                    }

                    // retain the fresh value, see if the selected cr is the one used to pay this si
                    if (collectionReceiptHeader.ServiceInvoiceId == sv.ServiceInvoiceId)
                    {
                        amountPaid -= collectionReceiptHeader.Total;
                        balance += collectionReceiptHeader.Total;
                    }
                }

                return Json(new
                {
                    Amount = sv.Total.ToString(SD.Two_Decimal_Format),
                    AmountPaid = amountPaid.ToString(SD.Two_Decimal_Format),
                    Balance = balance.ToString(SD.Two_Decimal_Format),
                    Ewt = withHoldingTaxAmount.ToString(SD.Two_Decimal_Format),
                    Wvat = withHoldingVatAmount.ToString(SD.Two_Decimal_Format),
                    Total = (sv.Total - (withHoldingTaxAmount + withHoldingVatAmount)).ToString(SD.Two_Decimal_Format)
                });
            }

            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> GetMultipleInvoiceDetails(int[] siNo, bool isSales, CancellationToken cancellationToken)
        {
            if (isSales)
            {
                var si = await _unitOfWork.FilprideSalesInvoice
                    .GetAsync(si => siNo.Contains(si.SalesInvoiceId), cancellationToken);

                if (si == null)
                {
                    return Json(null);
                }

                var vatType = si.CustomerOrderSlip?.VatType ?? si.Customer!.VatType;
                var hasEwt = si.CustomerOrderSlip?.HasEWT ?? si.Customer!.WithHoldingTax;
                var hasWvat = si.CustomerOrderSlip?.HasWVAT ?? si.Customer!.WithHoldingVat;

                var netDiscount = si.Amount - si.Discount;
                var netOfVatAmount = vatType == SD.VatType_Vatable
                    ? _unitOfWork.FilprideServiceInvoice.ComputeNetOfVat(netDiscount)
                    : netDiscount;
                var withHoldingTaxAmount = hasEwt
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m)
                    : 0;
                var withHoldingVatAmount = hasWvat
                    ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m)
                    : 0;

                return Json(new
                {
                    Amount = netDiscount,
                    si.AmountPaid,
                    si.Balance,
                    WithholdingTax = withHoldingTaxAmount,
                    WithholdingVat = withHoldingVatAmount,
                    Total = netDiscount - (withHoldingTaxAmount + withHoldingVatAmount)
                });
            }
            else
            {
                var sv = await _unitOfWork.FilprideServiceInvoice
                    .GetAsync(sv => siNo.Contains(sv.ServiceInvoiceId), cancellationToken);

                if (sv == null)
                {
                    return Json(null);
                }

                decimal netDiscount = sv.Total - sv.Discount;
                decimal netOfVatAmount = sv.VatType == SD.VatType_Vatable ? _unitOfWork.FilprideServiceInvoice.ComputeNetOfVat(netDiscount) : netDiscount;
                decimal withHoldingTaxAmount = sv.HasEwt ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m) : 0;
                decimal withHoldingVatAmount = sv.HasWvat ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m) : 0;

                return Json(new
                {
                    Amount = netDiscount,
                    sv.AmountPaid,
                    sv.Balance,
                    WithholdingTax = withHoldingTaxAmount,
                    WithholdingVat = withHoldingVatAmount,
                    Total = netDiscount - (withHoldingTaxAmount + withHoldingVatAmount)
                });
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> EditForSales(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();
            var minDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (await _unitOfWork.IsPeriodPostedAsync(Module.CollectionReceipt, existingModel.TransactionDate, cancellationToken))
            {
                throw new ArgumentException($"Cannot edit this record because the period {existingModel.TransactionDate:MMM yyyy} is already closed.");
            }

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptId == id)
                .ToListAsync(cancellationToken);

            var invoiceNo = invoicesPaid
                .Select(crd => crd.InvoiceNo);

            var viewModel = new CollectionReceiptSingleSiViewModel
            {
                CollectionReceiptId = existingModel.CollectionReceiptId,
                CustomerId = existingModel.CustomerId,
                Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken),
                TransactionDate = existingModel.TransactionDate,
                ReferenceNo = existingModel.ReferenceNo,
                Remarks = existingModel.Remarks,
                SalesInvoiceId = existingModel.SalesInvoiceId ?? 0,
                SalesInvoices = (await _unitOfWork.FilprideSalesInvoice
                        .GetAllAsync(si =>
                                si.Company == companyClaims &&
                                (
                                    (si.Balance > 0 || invoiceNo.Contains(si.SalesInvoiceNo!)) &&
                                    si.CustomerId == existingModel.CustomerId &&
                                    si.PostedBy != null
                                ), // <- always include if invoiceNo matches
                            cancellationToken))
                    .OrderBy(s => s.SalesInvoiceId)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SalesInvoiceId.ToString(),
                        Text = s.SalesInvoiceNo
                    })
                    .ToList(),
                CashAmount = existingModel.CashAmount,
                CheckDate = existingModel.CheckDate,
                CheckNo = existingModel.CheckNo,
                CheckBranch = existingModel.CheckBranch,
                CheckAmount = existingModel.CheckAmount,
                CheckBank = existingModel.CheckBank,
                ManagersCheckDate = existingModel.ManagersCheckDate,
                ManagersCheckNo = existingModel.ManagersCheckNo,
                ManagersCheckBank = existingModel.ManagersCheckBank,
                ManagersCheckBranch = existingModel.ManagersCheckBranch,
                ManagersCheckAmount = existingModel.ManagersCheckAmount,
                BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken),
                EWT = existingModel.EWT,
                WVAT = existingModel.WVAT,
                ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken),
                HasAlready2306 = existingModel.F2306FilePath != null,
                HasAlready2307 = existingModel.F2307FileName != null,
                MinDate = minDate,
                BatchNumber = existingModel.BatchNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForSales(CollectionReceiptSingleSiViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == viewModel.CollectionReceiptId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptNo == existingModel.CollectionReceiptNo)
                .Select(crd => crd.InvoiceNo)
                .ToListAsync(cancellationToken);

            viewModel.SalesInvoices = (await _unitOfWork.FilprideSalesInvoice.GetAllAsync(si => si.Company == companyClaims
                    && (si.Balance > 0 || invoicesPaid.Contains(si.SalesInvoiceNo!))
                    && si.CustomerId == existingModel.CustomerId
                    && si.PostedBy != null, cancellationToken))
                .OrderBy(s => s.SalesInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.SalesInvoiceId.ToString(),
                    Text = s.SalesInvoiceNo
                })
                .ToList();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["warning"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                var existingSalesInvoice = await _unitOfWork.FilprideSalesInvoice
                    .GetAsync(si => si.SalesInvoiceId == viewModel.SalesInvoiceId, cancellationToken);

                if (existingSalesInvoice == null)
                {
                    return NotFound();
                }

                // get existing details
                var detail = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (detail == null)
                {
                    throw new NullReferenceException("Collection Receipt Details Not Found.");
                }

                // based on details, revert the calculation done to sales invoices
                await _unitOfWork.FilprideCollectionReceipt.UndoSalesInvoiceChanges(detail, cancellationToken);

                existingModel.SalesInvoiceId = existingSalesInvoice.SalesInvoiceId;
                existingModel.SINo = existingSalesInvoice.SalesInvoiceNo;
                existingModel.CustomerId = viewModel.CustomerId;
                existingModel.TransactionDate = viewModel.TransactionDate;
                existingModel.ReferenceNo = viewModel.ReferenceNo;
                existingModel.Remarks = viewModel.Remarks;
                existingModel.CheckDate = viewModel.CheckDate;
                existingModel.CheckNo = viewModel.CheckNo;
                existingModel.CheckBranch = viewModel.CheckBranch;
                existingModel.CheckAmount = viewModel.CheckAmount;
                existingModel.CheckBank = viewModel.CheckBank;
                existingModel.ManagersCheckDate = viewModel.ManagersCheckDate;
                existingModel.ManagersCheckNo = viewModel.ManagersCheckNo;
                existingModel.ManagersCheckBank = viewModel.ManagersCheckBank;
                existingModel.ManagersCheckBranch = viewModel.ManagersCheckBranch;
                existingModel.ManagersCheckAmount = viewModel.ManagersCheckAmount;
                existingModel.CashAmount = viewModel.CashAmount;
                existingModel.EWT = viewModel.EWT;
                existingModel.WVAT = viewModel.WVAT;
                existingModel.Total = total;
                existingModel.BatchNumber = viewModel.BatchNumber;

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    existingModel.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    existingModel.F2306FilePath = await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, existingModel.F2306FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    existingModel.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    existingModel.F2307FilePath = await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, existingModel.F2307FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                if (!_dbContext.ChangeTracker.HasChanges())
                {
                    TempData["warning"] = "No data changes!";
                    return View(viewModel);
                }

                existingModel.EditedBy = GetUserFullName();
                existingModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                await _dbContext.CollectionReceiptDetails
                    .Where(x => x.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .ExecuteDeleteAsync(cancellationToken);

                var details = new CollectionReceiptDetail
                {
                    CollectionReceiptId = existingModel.CollectionReceiptId,
                    CollectionReceiptNo = existingModel.CollectionReceiptNo!,
                    InvoiceDate = DateOnly.FromDateTime(existingSalesInvoice.CreatedDate),
                    InvoiceNo = existingSalesInvoice.SalesInvoiceNo!,
                    Amount = existingModel.Total
                };

                await _dbContext.CollectionReceiptDetails.AddAsync(details, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                await _unitOfWork.FilprideCollectionReceipt.UpdateInvoice(existingModel.SalesInvoice!.SalesInvoiceId, existingModel.Total, cancellationToken);

                #endregion --Saving default value

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingModel.EditedBy!, $"Edited collection receipt# {existingModel.CollectionReceiptNo}", "Collection Receipt", existingModel.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                TempData["success"] = "Collection receipt successfully updated.";
                await transaction.CommitAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> EditForService(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var minDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            if (await _unitOfWork.IsPeriodPostedAsync(Module.CollectionReceipt, existingModel.TransactionDate, cancellationToken))
            {
                throw new ArgumentException($"Cannot edit this record because the period {existingModel.TransactionDate:MMM yyyy} is already closed.");
            }

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptId == id)
                .ToListAsync(cancellationToken);

            var invoiceNo = invoicesPaid
                .Select(crd => crd.InvoiceNo);

            var viewModel = new CollectionReceiptServiceViewModel
            {
                CollectionReceiptId = existingModel.CollectionReceiptId,
                CustomerId = existingModel.CustomerId,
                Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken),
                TransactionDate = existingModel.TransactionDate,
                ReferenceNo = existingModel.ReferenceNo,
                Remarks = existingModel.Remarks,
                ServiceInvoiceId = existingModel.ServiceInvoiceId ?? 0,
                ServiceInvoices = (await _unitOfWork.FilprideServiceInvoice
                        .GetAllAsync(si =>
                                si.Company == companyClaims &&
                                (
                                    (si.Balance > 0 || invoiceNo.Contains(si.ServiceInvoiceNo!)) &&
                                    si.CustomerId == existingModel.CustomerId &&
                                    si.PostedBy != null
                                ),
                            cancellationToken))
                    .OrderBy(si => si.ServiceInvoiceId)
                    .Select(s => new SelectListItem
                    {
                        Value = s.ServiceInvoiceId.ToString(),
                        Text = s.ServiceInvoiceNo
                    })
                    .ToList(),
                CashAmount = existingModel.CashAmount,
                CheckDate = existingModel.CheckDate,
                CheckNo = existingModel.CheckNo,
                CheckBank = existingModel.CheckBank,
                CheckBranch = existingModel.CheckBranch,
                CheckAmount = existingModel.CheckAmount,
                ManagersCheckDate = existingModel.ManagersCheckDate,
                ManagersCheckNo = existingModel.ManagersCheckNo,
                ManagersCheckBank = existingModel.ManagersCheckBank,
                ManagersCheckBranch = existingModel.ManagersCheckBranch,
                ManagersCheckAmount = existingModel.ManagersCheckAmount,
                BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken),
                EWT = existingModel.EWT,
                WVAT = existingModel.WVAT,
                ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken),
                HasAlready2306 = existingModel.F2306FilePath != null,
                HasAlready2307 = existingModel.F2307FileName != null,
                MinDate = minDate,
                BatchNumber = existingModel.BatchNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForService(CollectionReceiptServiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == viewModel.CollectionReceiptId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            viewModel.Customers = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims, cancellationToken);

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptNo == existingModel.CollectionReceiptNo)
                .Select(crd => crd.InvoiceNo)
                .ToListAsync(cancellationToken);

            viewModel.ServiceInvoices = (await _unitOfWork.FilprideServiceInvoice
                    .GetAllAsync(si => si.Company == companyClaims
                                       && (si.Balance > 0 || invoicesPaid.Contains(si.ServiceInvoiceNo!))
                                       && si.CustomerId == existingModel.CustomerId
                                       && si.PostedBy != null, cancellationToken))
                .OrderBy(si => si.ServiceInvoiceId)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceInvoiceId.ToString(),
                    Text = s.ServiceInvoiceNo
                })
                .ToList();

            viewModel.ChartOfAccounts = await _unitOfWork.GetChartOfAccountListAsyncByNo(cancellationToken);

            viewModel.BankAccounts = await _unitOfWork.GetFilprideBankAccountListById(companyClaims, cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

            var total = viewModel.CashAmount + viewModel.CheckAmount + viewModel.ManagersCheckAmount + viewModel.EWT + viewModel.WVAT;
            if (total == 0)
            {
                TempData["warning"] = "Please input at least one type form of payment";
                return View(viewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid!";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Saving default value

                var existingServiceInvoice = await _unitOfWork.FilprideServiceInvoice
                    .GetAsync(si => si.ServiceInvoiceId == viewModel.ServiceInvoiceId, cancellationToken);

                if (existingServiceInvoice == null)
                {
                    return NotFound();
                }

                var detail = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (detail == null)
                {
                    throw new NullReferenceException("Collection Receipt Details Not Found.");
                }

                await _unitOfWork.FilprideCollectionReceipt.UndoServiceInvoiceChanges(detail, cancellationToken);

                existingModel.ServiceInvoiceId = existingServiceInvoice.ServiceInvoiceId;
                existingModel.SVNo = existingServiceInvoice.ServiceInvoiceNo;
                existingModel.CustomerId = viewModel.CustomerId;
                existingModel.TransactionDate = viewModel.TransactionDate;
                existingModel.ReferenceNo = viewModel.ReferenceNo;
                existingModel.Remarks = viewModel.Remarks;
                existingModel.CheckDate = viewModel.CheckDate;
                existingModel.CheckNo = viewModel.CheckNo;
                existingModel.CheckBranch = viewModel.CheckBranch;
                existingModel.CheckAmount = viewModel.CheckAmount;
                existingModel.CheckBank = viewModel.CheckBank;
                existingModel.ManagersCheckDate = viewModel.ManagersCheckDate;
                existingModel.ManagersCheckNo = viewModel.ManagersCheckNo;
                existingModel.ManagersCheckBank = viewModel.ManagersCheckBank;
                existingModel.ManagersCheckBranch = viewModel.ManagersCheckBranch;
                existingModel.ManagersCheckAmount = viewModel.ManagersCheckAmount;
                existingModel.CashAmount = viewModel.CashAmount;
                existingModel.EWT = viewModel.EWT;
                existingModel.WVAT = viewModel.WVAT;
                existingModel.Total = total;
                existingModel.BatchNumber = viewModel.BatchNumber;

                if (viewModel.Bir2306 != null && viewModel.Bir2306.Length > 0)
                {
                    existingModel.F2306FileName = GenerateFileNameToSave(viewModel.Bir2306.FileName);
                    existingModel.F2306FilePath = await _cloudStorageService.UploadFileAsync(viewModel.Bir2306, existingModel.F2306FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                if (viewModel.Bir2307 != null && viewModel.Bir2307.Length > 0)
                {
                    existingModel.F2307FileName = GenerateFileNameToSave(viewModel.Bir2307.FileName);
                    existingModel.F2307FilePath = await _cloudStorageService.UploadFileAsync(viewModel.Bir2307, existingModel.F2307FileName!);
                    existingModel.IsCertificateUpload = true;
                }

                if (!_dbContext.ChangeTracker.HasChanges())
                {
                    TempData["warning"] = "No data changes!";
                    return View(viewModel);
                }

                existingModel.EditedBy = GetUserFullName();
                existingModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                await _dbContext.CollectionReceiptDetails
                    .Where(x => x.CollectionReceiptId == existingModel.CollectionReceiptId)
                    .ExecuteDeleteAsync(cancellationToken);

                var details = new CollectionReceiptDetail
                {
                    CollectionReceiptId = existingModel.CollectionReceiptId,
                    CollectionReceiptNo = existingModel.CollectionReceiptNo!,
                    InvoiceDate = DateOnly.FromDateTime(existingServiceInvoice.CreatedDate),
                    InvoiceNo = existingServiceInvoice.ServiceInvoiceNo,
                    Amount = existingModel.Total
                };

                await _dbContext.CollectionReceiptDetails.AddAsync(details, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                await _unitOfWork.FilprideCollectionReceipt.UpdateSV(existingModel.ServiceInvoice!.ServiceInvoiceId, existingModel.Total, 0, cancellationToken);

                #endregion --Saving default value

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingModel.EditedBy!, $"Edited collection receipt# {existingModel.CollectionReceiptNo}", "Collection Receipt", existingModel.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                TempData["success"] = "Collection receipt successfully updated.";
                await transaction.CommitAsync(cancellationToken);
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            if (await _unitOfWork.IsPeriodPostedAsync(Module.CollectionReceipt, model.TransactionDate, cancellationToken))
            {
                throw new ArgumentException($"Cannot post this record because the period {model.TransactionDate:MMM yyyy} is already closed.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.PostedBy = GetUserFullName();
                model.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(CollectionReceiptStatus.Posted);
                bool isMultipleSi = model.MultipleSIId?.Length > 0;

                await _unitOfWork.FilprideCollectionReceipt.PostAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.PostedBy!, $"Posted collection receipt# {model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been Posted.";

                return RedirectToAction(isMultipleSi ? nameof(MultipleCollectionPrint) : nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
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
            var model = await _unitOfWork.FilprideCollectionReceipt.GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

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
                model.Status = nameof(CollectionReceiptStatus.Voided);
                var series = model.SINo ?? model.SVNo;

                await _unitOfWork.FilprideCollectionReceipt.RemoveRecords<CashReceiptBook>(crb => crb.RefNo == model.CollectionReceiptNo, cancellationToken);
                await _unitOfWork.GeneralLedger.ReverseEntries(model.CollectionReceiptNo, cancellationToken);

                if (model.SINo != null)
                {
                    await _unitOfWork.FilprideCollectionReceipt.RemoveSIPayment(model.SalesInvoice!.SalesInvoiceId, model.Total, 0, cancellationToken);
                }
                else if (model.SVNo != null)
                {
                    await _unitOfWork.FilprideCollectionReceipt.RemoveSVPayment(model.ServiceInvoice!.ServiceInvoiceId, model.Total, 0, cancellationToken);
                }
                else if (model.MultipleSI != null)
                {
                    await _unitOfWork.FilprideCollectionReceipt.RemoveMultipleSIPayment(model.MultipleSIId!, model.SIMultipleAmount!, 0, cancellationToken);
                }
                else
                {
                    TempData["info"] = "No series number found";
                    return RedirectToAction(nameof(Index));
                }

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!, $"Voided collection receipt# {model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Collection Receipt #{model.CollectionReceiptNo} has been voided successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to void collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Voided by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // restore the changes to the SI
                var detail = await _dbContext.CollectionReceiptDetails
                    .Where(crd => crd.CollectionReceiptId == model.CollectionReceiptId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (detail == null)
                {
                    throw new NullReferenceException("Collection Receipt Details Not Found.");
                }

                if (model.SalesInvoiceId != null)
                {
                    await _unitOfWork.FilprideCollectionReceipt.UndoSalesInvoiceChanges(detail, cancellationToken);
                }
                else if (model.ServiceInvoiceId != null)
                {
                    await _unitOfWork.FilprideCollectionReceipt.UndoServiceInvoiceChanges(detail, cancellationToken);
                }
                else if (model.MultipleSIId != null)
                {
                    var listOfDetails = await _dbContext.CollectionReceiptDetails
                        .Where(crd => crd.CollectionReceiptId == model.CollectionReceiptId)
                        .ToListAsync(cancellationToken);

                    foreach (var details in listOfDetails)
                    {
                        await _unitOfWork.FilprideCollectionReceipt.UndoSalesInvoiceChanges(details, cancellationToken);
                    }
                }
                else
                {
                    throw new NullReferenceException("Collection Receipt Details Not Found.");
                }

                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(CollectionReceiptStatus.Canceled);
                model.CancellationRemarks = cancellationRemarks;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled collection receipt# {model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Collection Receipt #{model.CollectionReceiptNo} has been cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to cancel collection receipt. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var cr = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (cr == null)
            {
                return NotFound();
            }

            if (!cr.IsPrinted)
            {
                cr.IsPrinted = true;

                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(), $"Printed original copy of collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt", cr.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(), $"Printed re-printed copy of collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt", cr.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        public async Task<IActionResult> MultipleInvoiceBalance(int siNo, int? collectionReceiptId, CancellationToken cancellationToken)
        {
            var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                .GetAsync(si => si.SalesInvoiceId == siNo, cancellationToken);

            if (salesInvoice == null)
            {
                return Json(null);
            }

            var collectionsForThisSi = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.InvoiceNo == salesInvoice.SalesInvoiceNo
                              && crd.CollectionReceiptId == collectionReceiptId)
                .FirstOrDefaultAsync(cancellationToken);

            var vatType = salesInvoice.CustomerOrderSlip?.VatType ?? salesInvoice.Customer!.VatType;
            var hasEwt = salesInvoice.CustomerOrderSlip?.HasEWT ?? salesInvoice.Customer!.WithHoldingTax;
            var hasWvat = salesInvoice.CustomerOrderSlip?.HasWVAT ?? salesInvoice.Customer!.WithHoldingVat;

            var amount = salesInvoice.Amount;
            var amountPaid = salesInvoice.AmountPaid;
            var netDiscount = salesInvoice.Amount - salesInvoice.Discount;
            var netOfVatAmount = vatType == SD.VatType_Vatable
                ? _unitOfWork.FilprideCollectionReceipt.ComputeNetOfVat(netDiscount)
                : netDiscount;
            var vatAmount = vatType == SD.VatType_Vatable
                ? _unitOfWork.FilprideCollectionReceipt.ComputeVatAmount(netOfVatAmount)
                : 0m;
            var ewtAmount = hasEwt
                ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m)
                : 0m;
            var wvatAmount = hasWvat
                ? _unitOfWork.FilprideCollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m)
                : 0m;
            var balance = amount - amountPaid;

            if (collectionsForThisSi != null)
            {
                balance += collectionsForThisSi.Amount;
                amountPaid -= collectionsForThisSi.Amount;
            }

            return Json(new
            {
                Amount = amount,
                AmountPaid = amountPaid,
                NetAmount = netOfVatAmount,
                VatAmount = vatAmount,
                EwtAmount = ewtAmount,
                WvatAmount = wvatAmount,
                Balance = balance
            });
        }

        public async Task<IActionResult> MultipleCollectionPrint(int id, CancellationToken cancellationToken)
        {
            var cr = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (cr == null)
            {
                return NotFound();
            }

            return View(cr);
        }

        public async Task<IActionResult> PrintedMultipleCR(int id, CancellationToken cancellationToken)
        {
            var findIdOfCr = await _unitOfWork.FilprideCollectionReceipt.GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (findIdOfCr == null || findIdOfCr.IsPrinted)
            {
                return RedirectToAction(nameof(MultipleCollectionPrint), new { id });
            }

            findIdOfCr.IsPrinted = true;

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Printed original copy of collection receipt# {findIdOfCr.CollectionReceiptNo}", "Collection Receipt", findIdOfCr.Company);
            await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return RedirectToAction(nameof(MultipleCollectionPrint), new { id });
        }

        //Download as .xlsx file.(Export)

        [HttpGet]
        public async Task<IActionResult> GetAllCollectionReceiptIds()
        {
            var crIds = (await _unitOfWork.FilprideCollectionReceipt
                                     .GetAllAsync(cr => cr.Type == nameof(DocumentType.Documented)))
                                     .Select(cr => cr.CollectionReceiptId)
                                     .ToList();
            return Json(crIds);
        }

        [HttpGet]
        public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.DepositedDate = null;
                model.Status = nameof(CollectionReceiptStatus.Returned);

                await _unitOfWork.FilprideCollectionReceipt.ReturnedCheck(model.CollectionReceiptNo!, model.Company, GetUserFullName(), cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Return checks of collection receipt#{model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been returned successfully.";

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to returned checks. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Redeposit(int id, DateOnly redepositDate, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.DepositedDate = redepositDate;
                model.Status = nameof(CollectionReceiptStatus.Redeposited);

                await _unitOfWork.FilprideCollectionReceipt.RedepositAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Redeposit collection receipt#{model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been redeposited successfully.";

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to redeposit. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> ApplyClearingDate(int id, DateOnly clearingDate, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideCollectionReceipt
                .GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.ClearedDate = clearingDate;
                model.Status = nameof(CollectionReceiptStatus.Cleared);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Apply clearing date for collection receipt#{model.CollectionReceiptNo}", "Collection Receipt", model.Company);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt clearing date has been applied successfully.";

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to apply clearing date. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                if (model.SalesInvoiceId != null || model.MultipleSIId != null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(ServiceInvoiceIndex));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCollectionReceiptList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var companyClaims = await GetCompanyClaimAsync();

                var collectionReceipts = await _unitOfWork.FilprideCollectionReceipt
                    .GetAllAsync(sv => sv.Company == companyClaims && sv.Type == nameof(DocumentType.Documented), cancellationToken);

                // Apply date range filter if provided
                if (dateFrom.HasValue)
                {
                    collectionReceipts = collectionReceipts
                        .Where(s => s.TransactionDate >= DateOnly.FromDateTime(dateFrom.Value))
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    collectionReceipts = collectionReceipts
                        .Where(s => s.TransactionDate <= DateOnly.FromDateTime(dateTo.Value))
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    collectionReceipts = collectionReceipts
                        .Where(s =>
                            s.CollectionReceiptNo!.ToLower().Contains(searchValue) ||
                            s.TransactionDate.ToString(SD.Date_Format).ToLower().Contains(searchValue) ||
                            s.SINo?.ToLower().Contains(searchValue) == true ||
                            s.SVNo?.ToLower().Contains(searchValue) == true ||
                            (s.MultipleSI != null && s.MultipleSI.Any(si => si.ToLower().Contains(searchValue))) ||
                            s.Customer!.CustomerName.ToLower().Contains(searchValue) ||
                            s.Total.ToString().Contains(searchValue) ||
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

                    collectionReceipts = collectionReceipts
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = collectionReceipts.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<CollectionReceipt> pagedCollectionReceipts;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedCollectionReceipts = collectionReceipts;
                }
                else
                {
                    // Normal pagination
                    pagedCollectionReceipts = collectionReceipts
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedCollectionReceipts
                    .Select(x => new
                    {
                        x.CollectionReceiptId,
                        x.CollectionReceiptNo,
                        x.TransactionDate,
                        x.SINo,
                        x.MultipleSI,
                        x.SVNo,
                        customerName = x.Customer!.CustomerName,
                        x.Total,
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
                _logger.LogError(ex, "Failed to get collection receipts. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
