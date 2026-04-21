using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Common;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CustomerController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _dbContext;

        public CustomerController(IUnitOfWork unitOfWork, ILogger<CustomerController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.Customer
                .GetAllAsync(null, cancellationToken);

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var model = new Customer()
            {

                PaymentTerms = await _unitOfWork.Terms
                    .GetTermsListAsyncByCode(cancellationToken),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Make sure to fill all the required details.");
                return View(model);
            }

            model.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);

            var isTinExist = await _unitOfWork.Customer.IsTinNoExistAsync(model.CustomerTin, cancellationToken);

            if (isTinExist)
            {
                ModelState.AddModelError("CustomerTin", "Tin No already exist.");
                return View(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.CustomerCode = await _unitOfWork.Customer.GenerateCodeAsync(cancellationToken);
                model.CreatedBy = GetUserFullName();
                await _unitOfWork.Customer.AddAsync(model, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                #region -- Audit Trail Recording

                AuditTrail auditTrailBook = new(model.CreatedBy!,
                    $"Created new Customer #{model.CustomerCode}",
                    "Customer");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Customer created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create customer master file. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var customer = await _unitOfWork.Customer.GetAsync(c => c.CustomerId == id, cancellationToken);

            if (customer == null)
            {
                return NotFound();
            }

            customer.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);
            return View(customer);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.EditedBy = GetUserFullName();
                await _unitOfWork.Customer.UpdateAsync(model, cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (model.EditedBy,
                    $"Edited Customer #{model.CustomerCode}",
                    "Customer");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Customer updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to edit customer master file. Created by: {UserName}", _userManager.GetUserName(User));
                TempData["error"] = $"Error: '{ex.Message}'";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomersList([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var query = _unitOfWork.Customer
                    .GetAllQuery();

                var totalRecords = await query.CountAsync(cancellationToken);

                // Global search
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    query = query
                    .Where(c =>
                        c.CustomerCode!.ToLower().Contains(searchValue) ||
                        c.CustomerName.ToLower().Contains(searchValue) ||
                        c.CustomerTerms.ToLower().Contains(searchValue) ||
                        c.VatType.ToLower().Contains(searchValue)
                        );
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";
                    query = query
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await query.CountAsync(cancellationToken);
                var pagedData = await query
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
                _logger.LogError(ex, "Failed to get customer.");
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var customer = await _unitOfWork
                .Customer
                .GetAsync(c => c.CustomerId == id, cancellationToken);

            if (customer == null)
            {
                return NotFound();
            }

            customer.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);

            return View(customer);
        }

        [HttpPost, ActionName("Activate")]
        public async Task<IActionResult> ActivatePost(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var customer = await _unitOfWork
                .Customer
                .GetAsync(c => c.CustomerId == id, cancellationToken);

            if (customer == null)
            {
                return NotFound();
            }

            customer.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                customer.IsActive = true;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                var user = GetUserFullName();
                AuditTrail auditTrailBook = new(
                    user,
                    $"Activated Customer #{customer.CustomerCode}",
                    "Customer");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Customer has been activated";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate customer master file. Activated by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Activate), new { id = id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var customer = await _unitOfWork
                .Customer
                .GetAsync(c => c.CustomerId == id, cancellationToken);

            if (customer != null)
            {
                customer.PaymentTerms = await _unitOfWork.Terms
                    .GetTermsListAsyncByCode(cancellationToken);

                return View(customer);
            }

            return NotFound();
        }

        [HttpPost, ActionName("Deactivate")]
        public async Task<IActionResult> DeactivatePost(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var customer = await _unitOfWork
                .Customer
                .GetAsync(c => c.CustomerId == id, cancellationToken);

            if (customer == null)
            {
                return NotFound();
            }

            customer.PaymentTerms = await _unitOfWork.Terms
                .GetTermsListAsyncByCode(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                customer.IsActive = false;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region -- Audit Trail Recording --

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Deactivated Customer #{customer.CustomerCode}",
                    "Customer");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Customer has been deactivated";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate customer master file. Deactivated by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Deactivate), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCustomerList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _unitOfWork.Customer
                    .GetAllAsync(null, cancellationToken);

                // Apply date range filter if provided (using CreatedDate)
                if (dateFrom.HasValue)
                {
                    customers = customers
                        .Where(s => s.CreatedDate >= dateFrom.Value)
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    // Add one day to include the entire end date
                    var dateToInclusive = dateTo.Value.AddDays(1);
                    customers = customers
                        .Where(s => s.CreatedDate < dateToInclusive)
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    customers = customers
                        .Where(s =>
                            (s.CustomerCode != null && s.CustomerCode.ToLower().Contains(searchValue)) ||
                            (s.CustomerName != null && s.CustomerName.ToLower().Contains(searchValue)) ||
                            (s.CustomerTin != null && s.CustomerTin.ToLower().Contains(searchValue)) ||
                            (s.BusinessStyle != null && s.BusinessStyle.ToLower().Contains(searchValue)) ||
                            (s.CustomerTerms != null && s.CustomerTerms.ToLower().Contains(searchValue)) ||
                            s.CreatedDate.ToString("MMM dd, yyyy").ToLower().Contains(searchValue)
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
                        { "customerCode", "CustomerCode" },
                        { "customerName", "CustomerName" },
                        { "customerTin", "CustomerTin" },
                        { "businessStyle", "BusinessStyle" },
                        { "customerTerms", "CustomerTerms" },
                        { "customerType", "CustomerType" },
                        { "createdDate", "CreatedDate" }
                    };

                    // Get the actual property name
                    var actualColumnName = columnMapping.ContainsKey(columnName)
                        ? columnMapping[columnName]
                        : columnName;

                    customers = customers
                        .AsQueryable()
                        .OrderBy($"{actualColumnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = customers.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<Customer> pagedCustomers;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedCustomers = customers;
                }
                else
                {
                    // Normal pagination
                    pagedCustomers = customers
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedCustomers
                    .Select(x => new
                    {
                        x.CustomerId,
                        x.CustomerCode,
                        x.CustomerName,
                        x.CustomerTin,
                        x.BusinessStyle,
                        x.CustomerTerms,
                        x.CreatedDate
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
                _logger.LogError(ex, "Failed to get customers. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
