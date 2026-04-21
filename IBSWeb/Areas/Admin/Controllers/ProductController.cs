using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ProductController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _dbContext;

        public ProductController(IUnitOfWork unitOfWork, ILogger<ProductController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
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

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork
                .Product
                .GetAllAsync();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Make sure to fill all the required details.");
                return View(model);
            }

            bool IsProductExist = await _unitOfWork
                .Product
                .IsProductExist(model.ProductName, cancellationToken);

            if (IsProductExist)
            {
                ModelState.AddModelError("ProductName", "Product name already exist.");
                return View(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.CreatedBy = GetUserFullName();
                await _unitOfWork.Product.AddAsync(model, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                #region -- Audit Trail Recording --

                AuditTrail auditTrailBook = new (
                    GetUserFullName(), $"Created Product {model.ProductCode}",
                    "Product" );
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product master file. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetProductList([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var queried = await _unitOfWork.Product
                    .GetAllAsync(null, cancellationToken);

                // Global search
                if (!string.IsNullOrEmpty(parameters.Search?.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    queried = queried
                    .Where(p =>
                        p.ProductCode.ToLower().Contains(searchValue) == true ||
                        p.ProductName.ToLower().Contains(searchValue) == true ||
                        p.ProductUnit.ToLower().Contains(searchValue) == true
                        ).ToList();
                }

                // Sorting
                if (parameters.Order != null && parameters.Order.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";
                    queried = queried
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalRecords = queried.Count();
                var pagedData = queried
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
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
                _logger.LogError(ex, "Failed to get products.");
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetAsync(c => c.ProductId == id, cancellationToken);

            if (product != null)
            {
                return View(product);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _unitOfWork.Product.GetAsync(p => p.ProductId == model.ProductId, cancellationToken);

            if (existing == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.EditedBy = GetUserFullName();
                await _unitOfWork.Product.UpdateAsync(model, cancellationToken);

                #region -- Audit Trail Recording --

                AuditTrail auditTrailBook = new (
                    GetUserFullName(), $"Edited Product {existing.ProductCode} => {model.ProductCode}",
                    "Product" );
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in updating product.");
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = $"Error: '{ex.Message}'";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _unitOfWork
                .Product
                .GetAsync(c => c.ProductId == id, cancellationToken);

            if (product != null)
            {
                return View(product);
            }

            return NotFound();
        }

        [HttpPost, ActionName("Activate")]
        public async Task<IActionResult> ActivatePost(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product
                .GetAsync(c => c.ProductId == id, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                product.IsActive = true;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (
                    GetUserFullName(), $"Activated Product #{product.ProductCode}",
                    "Product");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Product activated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate product master file. Created by: {UserName}", _userManager.GetUserName(User));
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

            var product = await _unitOfWork
                .Product
                .GetAsync(c => c.ProductId == id, cancellationToken);

            if (product != null)
            {
                return View(product);
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

            var product = await _unitOfWork.Product
                .GetAsync(c => c.ProductId == id, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                product.IsActive = false;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (
                    GetUserFullName(), $"Deactivated Product #{product.ProductCode}",
                    "Product");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Product deactivated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate product master file. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Deactivate), new { id = id });
            }
        }

        [HttpGet]
        public IActionResult GetAllProductIds()
        {
            var productIds = _dbContext.Products
                                     .Select(p => p.ProductId) // Assuming Id is the primary key
                                     .ToList();

            return Json(productIds);
        }

        [HttpPost]
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            try
            {
                var products = (await _unitOfWork
                    .Product
                    .GetAllAsync(cancellationToken:  cancellationToken))
                    .Select(x => new
                    {
                        x.ProductId,
                        x.ProductCode,
                        x.ProductName,
                        x.ProductUnit,
                        x.CreatedBy,
                        x.CreatedDate
                    });

                return Json(new
                {
                    data = products
                });
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
