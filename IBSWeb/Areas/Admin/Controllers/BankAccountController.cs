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
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class BankAccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(IUnitOfWork unitOfWork, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<BankAccountController> logger)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _userManager = userManager;
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

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var banks = await _unitOfWork.BankAccount
                .GetAllAsync(null, cancellationToken);

                return View(banks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Index.");
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankAccount model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _unitOfWork.BankAccount.IsBankAccountNoExist(model.AccountNo, cancellationToken))
                {
                    ModelState.AddModelError("AccountNo", "Bank account no already exist!");
                    return View(model);
                }

                if (await _unitOfWork.BankAccount.IsBankAccountNameExist(model.AccountName, cancellationToken))
                {
                    ModelState.AddModelError("AccountName", "Bank account name already exist!");
                    return View(model);
                }

                var companyClaims = await GetCompanyClaimAsync();

                if (companyClaims == null)
                {
                    return BadRequest();
                }

                model.CreatedBy = GetUserFullName();

                await _unitOfWork.BankAccount.AddAsync(model, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                #region -- Audit Trail Recordings --

                AuditTrail auditTrailBook = new(model.CreatedBy!, $"Create new bank {model.Bank} {model.AccountName} {model.AccountNo}", "Bank Account");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recordings --

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Bank #{model.AccountNo} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create bank account. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBankAccountsList([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var query = _unitOfWork.BankAccount
                    .GetAllQuery();

                var totalRecords = await query.CountAsync(cancellationToken);

                // Global search
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasCreatedDate = DateTime.TryParse(searchValue, out var createdDateTime);

                    query = query
                    .Where(b =>
                        b.AccountNo.ToLower().Contains(searchValue) ||
                        b.AccountName.ToLower().Contains(searchValue) ||
                        b.Bank.ToLower().Contains(searchValue) ||
                        b.Branch.ToLower().Contains(searchValue) ||
                        b.CreatedBy!.ToLower().Contains(searchValue) ||
                        (hasCreatedDate && DateOnly.FromDateTime(b.CreatedDate) == DateOnly.FromDateTime(createdDateTime))
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
                _logger.LogError(ex, "Failed to get bank accounts.");
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.BankAccount
                .GetAsync(b => b.BankAccountId == id, cancellationToken);
            return View(existingModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BankAccount model, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.BankAccount
                .GetAsync(b => b.BankAccountId == model.BankAccountId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "The information you submitted is not valid!");
                return View(existingModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region -- Audit Trail Recordings --

                AuditTrail auditTrailBook = new(
                    GetUserFullName(),
                    $"Edited bank {existingModel.Bank} {existingModel.AccountName} {existingModel.AccountNo} => {model.Bank} {model.AccountName} {model.AccountNo}",
                    "Bank Account");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail Recordings --

                existingModel.AccountNo = model.AccountNo;
                existingModel.AccountName = model.AccountName;
                existingModel.Bank = model.Bank;
                existingModel.Branch = model.Branch;

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Bank edited successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit bank account. Edited by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(existingModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBankAccountList(CancellationToken cancellationToken)
        {
            try
            {
                var bankAccounts = (await _unitOfWork.BankAccount
                    .GetAllAsync(null, cancellationToken))
                    .Select(x => new
                    {
                        x.BankAccountId,
                        x.AccountNo,
                        x.AccountName,
                        x.Bank,
                        x.Branch,
                        x.CreatedBy,
                        x.CreatedDate
                    });

                return Json(new
                {
                    data = bankAccounts
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
