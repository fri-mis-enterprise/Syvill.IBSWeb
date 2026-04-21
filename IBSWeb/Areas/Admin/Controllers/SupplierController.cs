using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Common;
using IBS.Models.MasterFile;
using IBS.Services;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupplierController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly ICacheService _cacheService;

        public SupplierController(IUnitOfWork unitOfWork,
            ILogger<SupplierController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            ICloudStorageService cloudStorageService,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
            _cloudStorageService = cloudStorageService;
            _cacheService = cacheService;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            Supplier model = new()
            {
                DefaultExpenses = await _dbContext.ChartOfAccounts
                    .Where(coa => !coa.HasChildren)
                    .OrderBy(coa => coa.AccountNumber)
                    .Select(s => new SelectListItem
                    {
                        Value = s.AccountNumber,
                        Text = s.AccountNumber + " " + s.AccountName
                    })
                    .ToListAsync(cancellationToken),
                WithholdingTaxList = await _dbContext.ChartOfAccounts
                    .Where(coa => coa.AccountNumber!.Contains("2010302") && !coa.HasChildren)
                    .OrderBy(coa => coa.AccountNumber)
                    .Select(s => new SelectListItem
                    {
                        Value = s.AccountNumber + " " + s.AccountName,
                        Text = s.AccountNumber + " " + s.AccountName
                    })
                    .ToListAsync(cancellationToken),
                PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier model, IFormFile? registration, IFormFile? document, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            model.DefaultExpenses = await _dbContext.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            model.WithholdingTaxList = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountNumber!.Contains("2010302") && !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber + " " + s.AccountName,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            model.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Make sure to fill all the required details.");
                return View(model);
            }

            if (await _unitOfWork.Supplier.IsSupplierExistAsync(model.SupplierName, model.Category,
                    companyClaims, cancellationToken))
            {
                ModelState.AddModelError("SupplierName", "Supplier already exist.");
                return View(model);
            }

            if (await _unitOfWork.Supplier.IsTinNoExistAsync(model.SupplierTin, model.Branch!,
                    model.Category, companyClaims, cancellationToken))
            {
                ModelState.AddModelError("SupplierTin", "Tin number already exist.");
                return View(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (registration != null && registration.Length > 0)
                {
                    model.ProofOfRegistrationFileName = GenerateFileNameToSave(registration.FileName);
                    model.ProofOfRegistrationFilePath = await _cloudStorageService.UploadFileAsync(registration, model.ProofOfRegistrationFileName!);
                }

                if (document != null && document.Length > 0)
                {
                    model.ProofOfExemptionFileName = GenerateFileNameToSave(document.FileName);
                    model.ProofOfExemptionFilePath = await _cloudStorageService.UploadFileAsync(document, model.ProofOfExemptionFileName!);
                }

                model.SupplierCode = await _unitOfWork.Supplier.GenerateCodeAsync(cancellationToken);
                model.CreatedBy = GetUserFullName();
                await _unitOfWork.Supplier.AddAsync(model, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                #region -- Audit Trail Recording --

                AuditTrail auditTrailBook = new(model.CreatedBy!,
                    $"Create new Supplier #{model.SupplierCode}",
                    "Supplier");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Supplier created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create supplier master file. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = $"Error: '{ex.Message}'";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSuppliersList([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var queried = _unitOfWork.Supplier
                    .GetAllQuery();

                var totalRecords = await queried.CountAsync(cancellationToken);

                // Global search
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    queried = queried
                    .Where(s =>
                        s.SupplierCode!.ToLower().Contains(searchValue) ||
                        s.SupplierName.ToLower().Contains(searchValue) ||
                        s.SupplierAddress.ToLower().Contains(searchValue) ||
                        s.SupplierTin.ToLower().Contains(searchValue) ||
                        s.SupplierTerms.ToLower().Contains(searchValue) ||
                        s.Category.ToLower().Contains(searchValue)
                        );
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Data;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    queried = queried
                        .OrderBy($"{columnName} {sortDirection}") ;
                }

                var totalFilteredRecords = await queried.CountAsync(cancellationToken);
                var pagedData = await queried
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
                _logger.LogError(ex, "Failed to get suppliers.");
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

            var supplier = await _unitOfWork.Supplier.GetAsync(c => c.SupplierId == id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            supplier.DefaultExpenses = await _dbContext.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            supplier.WithholdingTaxList = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountNumber!.Contains("2010302") && !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber + " " + s.AccountName,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            supplier.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier model, IFormFile? registration, IFormFile? document, CancellationToken cancellationToken)
        {
            model.DefaultExpenses = await _dbContext.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            model.WithholdingTaxList = await _dbContext.ChartOfAccounts
                .Where(coa => coa.AccountNumber!.Contains("2010302") && !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber + " " + s.AccountName,
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);

            model.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (registration != null && registration.Length > 0)
                {
                    model.ProofOfRegistrationFileName = GenerateFileNameToSave(registration.FileName);
                    model.ProofOfRegistrationFilePath = await _cloudStorageService.UploadFileAsync(registration, model.ProofOfRegistrationFileName!);
                }

                if (document != null && document.Length > 0)
                {
                    model.ProofOfExemptionFileName = GenerateFileNameToSave(document.FileName);
                    model.ProofOfExemptionFilePath = await _cloudStorageService.UploadFileAsync(document, model.ProofOfExemptionFileName!);
                }

                model.EditedBy = GetUserFullName();
                await _unitOfWork.Supplier.UpdateAsync(model, cancellationToken);

                #region -- Audit Trail Recording --

                AuditTrail auditTrailBook = new (GetUserFullName(),
                    $"Edited Supplier #{model.SupplierCode}",
                    "Supplier" );
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recording --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Supplier updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit supplier master file. Edited by: {UserName}", _userManager.GetUserName(User));
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

            var supplier = await _unitOfWork.Supplier.GetAsync(c => c.SupplierId == id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            supplier.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            return View(supplier);
        }

        [HttpPost, ActionName("Activate")]
        public async Task<IActionResult> ActivatePost(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var supplier = await _unitOfWork.Supplier.GetAsync(c => c.SupplierId == id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            supplier.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                supplier.IsActive = true;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new(GetUserFullName(),
                    $"Activated Supplier #{supplier.SupplierCode}",
                    "Supplier");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Supplier activated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate supplier master file. Activated by: {UserName}", _userManager.GetUserName(User));
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

            var supplier = await _unitOfWork.Supplier
                .GetAsync(c => c.SupplierId == id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            supplier.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            return View(supplier);
        }

        [HttpPost, ActionName("Deactivate")]
        public async Task<IActionResult> DeactivatePost(int? id, CancellationToken cancellationToken)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var supplier = await _unitOfWork.Supplier.GetAsync(c => c.SupplierId == id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            supplier.PaymentTerms = await _unitOfWork.Terms.GetTermsListAsyncByCode(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                supplier.IsActive = false;
                await _unitOfWork.SaveAsync(cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (GetUserFullName(),
                    $"Deactivated Supplier #{supplier.SupplierCode}",
                    "Supplier");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Supplier deactivated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate supplier master file. Deactivated by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Deactivate), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetSupplierList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var suppliers = await _unitOfWork.Supplier
                    .GetAllAsync(null, cancellationToken);

                // Apply date range filter if provided (using CreatedDate)
                if (dateFrom.HasValue)
                {
                    suppliers = suppliers
                        .Where(s => s.CreatedDate >= dateFrom.Value)
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    // Add one day to include the entire end date
                    var dateToInclusive = dateTo.Value.AddDays(1);
                    suppliers = suppliers
                        .Where(s => s.CreatedDate < dateToInclusive)
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    suppliers = suppliers
                        .Where(s =>
                            (s.SupplierCode != null && s.SupplierCode.ToLower().Contains(searchValue)) ||
                            (s.SupplierName != null && s.SupplierName.ToLower().Contains(searchValue)) ||
                            (s.SupplierAddress != null && s.SupplierAddress.ToLower().Contains(searchValue)) ||
                            (s.SupplierTin != null && s.SupplierTin.ToLower().Contains(searchValue)) ||
                            (s.SupplierTerms != null && s.SupplierTerms.ToLower().Contains(searchValue)) ||
                            (s.VatType != null && s.VatType.ToLower().Contains(searchValue)) ||
                            (s.Category != null && s.Category.ToLower().Contains(searchValue)) ||
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
                        { "supplierCode", "SupplierCode" },
                        { "supplierName", "SupplierName" },
                        { "supplierAddress", "SupplierAddress" },
                        { "supplierTin", "SupplierTin" },
                        { "supplierTerms", "SupplierTerms" },
                        { "vatType", "VatType" },
                        { "category", "Category" },
                        { "createdDate", "CreatedDate" }
                    };

                    // Get the actual property name
                    var actualColumnName = columnMapping.ContainsKey(columnName)
                        ? columnMapping[columnName]
                        : columnName;

                    suppliers = suppliers
                        .AsQueryable()
                        .OrderBy($"{actualColumnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = suppliers.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<Supplier> pagedSuppliers;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedSuppliers = suppliers;
                }
                else
                {
                    // Normal pagination
                    pagedSuppliers = suppliers
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedSuppliers
                    .Select(x => new
                    {
                        x.SupplierId,
                        x.SupplierCode,
                        x.SupplierName,
                        x.SupplierAddress,
                        x.SupplierTin,
                        x.SupplierTerms,
                        x.VatType,
                        x.Category,
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
                _logger.LogError(ex, "Failed to get suppliers. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
