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

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            ViewBag.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.CollectionReceipt, cancellationToken);

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

                var collectionReceipts = _unitOfWork.CollectionReceipt
                    .GetAllQuery();

                var totalRecords = await collectionReceipts.CountAsync(cancellationToken);

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

        public async Task<IActionResult> GetBanks(CancellationToken cancellationToken = default)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            return Json(await _unitOfWork.GetBankAccountListById(companyClaims, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> Deposit(int id, int bankId, DateOnly depositDate, CancellationToken cancellationToken)
        {
            var bank = await _unitOfWork.BankAccount
                .GetAsync(b => b.BankAccountId == bankId, cancellationToken);

            if (bank == null)
            {
                return NotFound();
            }

            var model = await _unitOfWork.CollectionReceipt
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

                await _unitOfWork.CollectionReceipt.DepositAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Record deposit date of collection receipt#{model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt deposited date has been recorded successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to record deposit date. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Print(int id, CancellationToken cancellationToken)
        {
            var cr = await _unitOfWork.CollectionReceipt.GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

            if (cr == null)
            {
                return NotFound();
            }

            var companyClaims = await GetCompanyClaimAsync();

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(cr);
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

                invoices = (await _unitOfWork.ServiceInvoice
                        .GetAllAsync(si =>
                                (si.Balance > 0 || invoiceNo.Contains(si.ServiceInvoiceNo!)) &&
                                si.CustomerId == customerNo &&
                                si.PostedBy != null,
                            cancellationToken))
                    .OrderBy(si => si.ServiceInvoiceId)
                    .ToList();
            }
            else
            {
                invoices = (await _unitOfWork.ServiceInvoice
                        .GetAllAsync(si => si.CustomerId == customerNo
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
            var sv = await _unitOfWork.ServiceInvoice
                    .GetAsync(s => s.ServiceInvoiceId == invoiceNo, cancellationToken);

            if (sv == null)
            {
                return NotFound();
            }

            var netOfVatAmount = sv.VatType == SD.VatType_Vatable
                ? _unitOfWork.ServiceInvoice.ComputeNetOfVat(sv.Total) - sv.Discount
                : sv.Total - sv.Discount;
            var withHoldingTaxAmount = sv.HasEwt
                ? _unitOfWork.CollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.01m)
                : 0;
            var withHoldingVatAmount = sv.HasWvat
                ? _unitOfWork.CollectionReceipt.ComputeEwtAmount(netOfVatAmount, 0.05m)
                : 0;
            var balance = sv.Balance;
            var amountPaid = sv.AmountPaid;

            // it means it is in edit
            if (crId != null)
            {
                // get the current amount of this cr
                var collectionReceiptHeader = await _unitOfWork.CollectionReceipt
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


        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }
            var existingModel = await _unitOfWork.CollectionReceipt
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
                Customers = await _unitOfWork.GetCustomerListAsyncById(companyClaims, cancellationToken),
                TransactionDate = existingModel.TransactionDate,
                ReferenceNo = existingModel.ReferenceNo,
                Remarks = existingModel.Remarks,
                ServiceInvoiceId = existingModel.ServiceInvoiceId,
                ServiceInvoices = (await _unitOfWork.ServiceInvoice
                        .GetAllAsync(si =>
                                (si.Balance > 0 || invoiceNo.Contains(si.ServiceInvoiceNo!)) &&
                                si.CustomerId == existingModel.CustomerId &&
                                si.PostedBy != null,
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
                BankAccounts = await _unitOfWork.GetBankAccountListById(companyClaims, cancellationToken),
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
        public async Task<IActionResult> Edit(CollectionReceiptServiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.CollectionReceipt
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

            viewModel.Customers = await _unitOfWork.GetCustomerListAsyncById(companyClaims, cancellationToken);

            var invoicesPaid = await _dbContext.CollectionReceiptDetails
                .Where(crd => crd.CollectionReceiptNo == existingModel.CollectionReceiptNo)
                .Select(crd => crd.InvoiceNo)
                .ToListAsync(cancellationToken);

            viewModel.ServiceInvoices = (await _unitOfWork.ServiceInvoice
                    .GetAllAsync(si => (si.Balance > 0 || invoicesPaid.Contains(si.ServiceInvoiceNo!))
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

            viewModel.BankAccounts = await _unitOfWork.GetBankAccountListById(companyClaims, cancellationToken);

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

                var existingServiceInvoice = await _unitOfWork.ServiceInvoice
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

                await _unitOfWork.CollectionReceipt.UndoServiceInvoiceChanges(detail, cancellationToken);

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

                await _unitOfWork.CollectionReceipt.UpdateSV(existingModel.ServiceInvoice!.ServiceInvoiceId, existingModel.Total, cancellationToken);

                #endregion --Saving default value

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingModel.EditedBy!, $"Edited collection receipt# {existingModel.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

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
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.CollectionReceipt
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

                await _unitOfWork.CollectionReceipt.PostAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.PostedBy!, $"Posted collection receipt# {model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been Posted.";

                return RedirectToAction(nameof(Print), new { id });
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
            var model = await _unitOfWork.CollectionReceipt.GetAsync(cr => cr.CollectionReceiptId == id, cancellationToken);

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

                await _unitOfWork.GeneralLedger.ReverseEntries(model.CollectionReceiptNo, cancellationToken);

                await _unitOfWork.CollectionReceipt.RemoveSVPayment(model.ServiceInvoiceId, model.Total, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!, $"Voided collection receipt# {model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

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
            var model = await _unitOfWork.CollectionReceipt
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

                await _unitOfWork.CollectionReceipt.UndoServiceInvoiceChanges(detail, cancellationToken);

                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(CollectionReceiptStatus.Canceled);
                model.CancellationRemarks = cancellationRemarks;

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled collection receipt# {model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

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
            var cr = await _unitOfWork.CollectionReceipt
                .GetAsync(x => x.CollectionReceiptId == id, cancellationToken);

            if (cr == null)
            {
                return NotFound();
            }

            if (!cr.IsPrinted)
            {
                cr.IsPrinted = true;

                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(), $"Printed original copy of collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrail = new(GetUserFullName(), $"Printed re-printed copy of collection receipt# {cr.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrail, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.CollectionReceipt
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

                await _unitOfWork.CollectionReceipt.ReturnedCheck(model.CollectionReceiptNo, "", GetUserFullName(), cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Return checks of collection receipt#{model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been returned successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to returned checks. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Redeposit(int id, DateOnly redepositDate, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.CollectionReceipt
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

                await _unitOfWork.CollectionReceipt.RedepositAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Redeposit collection receipt#{model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt has been redeposited successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to redeposit. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(Index));
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> ApplyClearingDate(int id, DateOnly clearingDate, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.CollectionReceipt
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
                    $"Apply clearing date for collection receipt#{model.CollectionReceiptNo}", "Collection Receipt");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Collection Receipt clearing date has been applied successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to apply clearing date. Error: {ErrorMessage}, Stack: {StackTrace}. Recorded by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(Index));
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

                var collectionReceipts = await _unitOfWork.CollectionReceipt
                    .GetAllAsync(sv => sv.Type == nameof(DocumentType.Documented), cancellationToken);

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
                            s.SVNo?.ToLower().Contains(searchValue) == true ||
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
