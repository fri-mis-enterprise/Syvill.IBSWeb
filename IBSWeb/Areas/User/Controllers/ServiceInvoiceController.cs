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
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_Finance, SD.Department_RCD)]
    public class ServiceInvoiceController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ServiceInvoiceController> _logger;

        public ServiceInvoiceController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<ServiceInvoiceController> logger)
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

        public IActionResult Index(string? view)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetServiceInvoices([FromForm] DataTablesParameters parameters, DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {
                var serviceInvoices = _unitOfWork.ServiceInvoice
                    .GetAllQuery();

                var totalRecords = await serviceInvoices.CountAsync(cancellationToken);

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasPeriod = DateOnly.TryParse(searchValue, out var period);

                    serviceInvoices = serviceInvoices
                        .Where(s =>
                            s.ServiceInvoiceNo.ToLower().Contains(searchValue) ||
                            s.CustomerName.ToLower().Contains(searchValue) ||
                            s.ServiceName.ToLower().Contains(searchValue) ||
                            (hasPeriod && s.Period == period) ||
                            s.Total.ToString().Contains(searchValue) ||
                            s.Instructions.ToLower().Contains(searchValue) ||
                            s.CreatedBy!.ToLower().Contains(searchValue) == true
                            );
                }
                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    serviceInvoices = serviceInvoices.Where(s => s.Period == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    serviceInvoices = serviceInvoices
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await serviceInvoices.CountAsync(cancellationToken);

                var pagedData = await serviceInvoices
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
                _logger.LogError(ex, "Failed to get service invoice. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var viewModel = new ServiceInvoiceViewModel
            {
                Customers = await _unitOfWork.GetCustomerListAsyncById(cancellationToken),
                Services = await _unitOfWork.GetServiceListById(cancellationToken)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceInvoiceViewModel viewModel, CancellationToken cancellationToken)
        {
            viewModel.Customers = await _unitOfWork.GetCustomerListAsyncById(cancellationToken);
            viewModel.Services = await _unitOfWork.GetServiceListById(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Retrieval of Customer/Service

                var customer = await _unitOfWork.Customer
                    .GetAsync(c => c.CustomerId == viewModel.CustomerId, cancellationToken);

                var service = await _unitOfWork.Service
                    .GetAsync(c => c.ServiceId == viewModel.ServiceId, cancellationToken);

                if (customer == null || service == null)
                {
                    return NotFound();
                }

                #endregion --Retrieval of Customer/Service

                var model = new ServiceInvoice
                {
                    ServiceInvoiceNo = await _unitOfWork.ServiceInvoice.GenerateCodeAsync(viewModel.Type, cancellationToken),
                    ServiceId = service.ServiceId,
                    ServiceName = service.Name,
                    ServicePercent = service.Percent,
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    CustomerAddress = customer.CustomerAddress,
                    CustomerBusinessStyle = customer.BusinessStyle,
                    CustomerTin = customer.CustomerTin,
                    VatType = service.Name != "TRANSACTION FEE" ? customer.VatType : SD.VatType_Exempt,
                    HasEwt = customer.WithHoldingTax && service.Name != "TRANSACTION FEE",
                    HasWvat = customer.WithHoldingVat && service.Name != "TRANSACTION FEE",
                    CreatedBy = GetUserFullName(),
                    Total = viewModel.Total,
                    Balance = viewModel.Total,
                    Period = viewModel.Period,
                    Instructions = viewModel.Instructions,
                    DueDate = viewModel.DueDate,
                    Discount = viewModel.Discount,
                    Type = viewModel.Type,
                };

                await _unitOfWork.ServiceInvoice.AddAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!, $"Created new service invoice# {model.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                TempData["success"] = $"Service invoice #{model.ServiceInvoiceNo} created successfully.";
                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create service invoice. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Print(int id, CancellationToken cancellationToken)
        {
            var sv = await _unitOfWork.ServiceInvoice
                .GetAsync(s => s.ServiceInvoiceId == id, cancellationToken);

            if (sv == null)
            {
                return NotFound();
            }

            #region --Audit Trail Recording

            AuditTrail auditTrailBook = new(GetUserFullName(), $"Preview service invoice#{sv.ServiceInvoiceNo}", "Service Invoice");
            await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(sv);
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.ServiceInvoice
                .GetAsync(s => s.ServiceInvoiceId == id, cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (model == null)
                {
                    return NotFound();
                }

                model.PostedBy = GetUserFullName();
                model.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Posted);

                await _unitOfWork.ServiceInvoice.PostAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.PostedBy!, $"Posted service invoice# {model.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Service invoice has been posted.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post service invoice. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.ServiceInvoice.GetAsync(x => x.ServiceInvoiceId == id, cancellationToken);

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

                AuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled service invoice# {model.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Service Invoice #{model.ServiceInvoiceNo} has been cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to cancel service invoice. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.ServiceInvoice.GetAsync(x => x.ServiceInvoiceId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            var hasAlreadyBeenUsed =
                await _dbContext.CollectionReceipts.AnyAsync(cr => cr.ServiceInvoiceId == model.ServiceInvoiceId && cr.Status != nameof(Status.Voided), cancellationToken) ||
                await _dbContext.DebitMemos.AnyAsync(dm => dm.ServiceInvoiceId == model.ServiceInvoiceId && dm.Status != nameof(Status.Voided), cancellationToken) ||
                await _dbContext.CreditMemos.AnyAsync(cm => cm.ServiceInvoiceId == model.ServiceInvoiceId && cm.Status != nameof(Status.Voided), cancellationToken);

            if (hasAlreadyBeenUsed)
            {
                TempData["info"] = "Please note that this record has already been utilized in a collection receipts, debit or credit memo. As a result, voiding it is not permitted.";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.PostedBy = null;
                model.VoidedBy = GetUserFullName();
                model.VoidedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Voided);

                await _unitOfWork.GeneralLedger.ReverseEntries(model.ServiceInvoiceNo, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(model.VoidedBy!, $"Voided service invoice# {model.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Service Invoice #{model.ServiceInvoiceNo} has been voided successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to void service invoice. Error: {ErrorMessage}, Stack: {StackTrace}. Voided by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [DepartmentAuthorize(SD.Department_CreditAndCollection, SD.Department_RCD)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.ServiceInvoice
                .GetAsync(sv => sv.ServiceInvoiceId == id, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            var viewModel = new ServiceInvoiceViewModel
            {
                ServiceInvoiceId = existingModel.ServiceInvoiceId,
                CustomerId = existingModel.CustomerId,
                Customers = await _unitOfWork.GetCustomerListAsyncById(cancellationToken),
                ServiceId = existingModel.ServiceId,
                Services = await _unitOfWork.GetServiceListById(cancellationToken),
                DueDate = existingModel.DueDate,
                Instructions = existingModel.Instructions,
                Period = existingModel.Period,
                Total = existingModel.Total,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceInvoiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.ServiceInvoice
                .GetAsync(s => s.ServiceInvoiceId == viewModel.ServiceInvoiceId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            viewModel.Customers = await _unitOfWork.GetCustomerListAsyncById(cancellationToken);
            viewModel.Services = await _unitOfWork.GetServiceListById(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var customer = await _unitOfWork.Customer
                    .GetAsync(c => c.CustomerId == viewModel.CustomerId, cancellationToken);

                var service = await _unitOfWork.Service
                    .GetAsync(c => c.ServiceId == viewModel.ServiceId, cancellationToken);

                if (customer == null || service == null)
                {
                    return NotFound();
                }

                #region --Saving the default properties

                existingModel.Discount = viewModel.Discount;
                existingModel.Total = viewModel.Total;
                existingModel.Balance = viewModel.Total;
                existingModel.Period = viewModel.Period;
                existingModel.DueDate = viewModel.DueDate;
                existingModel.Instructions = viewModel.Instructions;
                existingModel.EditedBy = GetUserFullName();
                existingModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                existingModel.Total = viewModel.Total;
                existingModel.CustomerId = viewModel.CustomerId;
                existingModel.ServiceId = viewModel.ServiceId;
                existingModel.ServiceName = service.Name;
                existingModel.ServicePercent = service.Percent;
                existingModel.CustomerName = customer.CustomerName;
                existingModel.CustomerBusinessStyle = customer.BusinessStyle;
                existingModel.CustomerAddress = customer.CustomerAddress;
                existingModel.CustomerTin = customer.CustomerTin;
                existingModel.VatType = service.Name != "TRANSACTION FEE" ? customer.VatType : SD.VatType_Exempt;
                existingModel.HasEwt = customer.WithHoldingTax && service.Name != "TRANSACTION FEE";
                existingModel.HasWvat = customer.WithHoldingVat && service.Name != "TRANSACTION FEE";

                #endregion --Saving the default properties

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(existingModel.EditedBy!, $"Edited service invoice# {existingModel.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Service invoice updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit service invoice. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var sv = await _unitOfWork.ServiceInvoice
                .GetAsync(x => x.ServiceInvoiceId == id, cancellationToken);

            if (sv == null)
            {
                return NotFound();
            }

            if (!sv.IsPrinted)
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(), $"Printed original copy of service invoice# {sv.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                sv.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(), $"Printed re-printed copy of service invoice# {sv.ServiceInvoiceNo}", "Service Invoice");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetServiceInvoiceList(
                [FromForm] DataTablesParameters parameters,
                string? dateFrom,  // Format: "2024-01" (year-month)
                string? dateTo,    // Format: "2024-12" (year-month)
                CancellationToken cancellationToken)
        {
            try
            {
                var serviceInvoices = await _unitOfWork.ServiceInvoice
                    .GetAllAsync(sv => sv.Type == nameof(DocumentType.Documented), cancellationToken);

                // Apply month range filter if provided
                if (!string.IsNullOrEmpty(dateFrom))
                {
                    // Parse "2024-01" to first day of month
                    var parts = dateFrom.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
                    {
                        var fromDate = new DateOnly(year, month, 1);
                        serviceInvoices = serviceInvoices
                            .Where(s => s.Period >= fromDate)
                            .ToList();
                    }
                }

                if (!string.IsNullOrEmpty(dateTo))
                {
                    // Parse "2024-12" to last day of month
                    var parts = dateTo.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
                    {
                        var daysInMonth = DateTime.DaysInMonth(year, month);
                        var toDate = new DateOnly(year, month, daysInMonth);
                        serviceInvoices = serviceInvoices
                            .Where(s => s.Period <= toDate)
                            .ToList();
                    }
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    serviceInvoices = serviceInvoices
                        .Where(s =>
                            s.ServiceInvoiceNo!.ToLower().Contains(searchValue) ||
                            s.CustomerName!.ToLower().Contains(searchValue) ||
                            s.ServiceName!.ToLower().Contains(searchValue) ||
                            s.Period.ToString("MMM yyyy").ToLower().Contains(searchValue) ||
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

                    serviceInvoices = serviceInvoices
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = serviceInvoices.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<ServiceInvoice> pagedServiceInvoices;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedServiceInvoices = serviceInvoices;
                }
                else
                {
                    // Normal pagination
                    pagedServiceInvoices = serviceInvoices
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedServiceInvoices
                    .Select(x => new
                    {
                        x.ServiceInvoiceId,
                        x.ServiceInvoiceNo,
                        x.CustomerName,
                        x.ServiceName,
                        x.Period,
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
                _logger.LogError(ex, "Failed to get service invoices. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReJournalService(int? month, int? year, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var serviceInvoices = await _unitOfWork.ServiceInvoice
                    .GetAllAsync(x =>
                            x.Status == nameof(Status.Posted) &&
                            x.Period.Month == month &&
                            x.Period.Year == year,
                        cancellationToken);

                if (!serviceInvoices.Any())
                {
                    return Json(new { sucess = true, message = "No records were returned." });
                }

                foreach (var service in serviceInvoices
                             .OrderBy(x => x.Period))
                {
                    await _unitOfWork.ServiceInvoice.PostAsync(service, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Json(new { month, year, count = serviceInvoices.Count() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
