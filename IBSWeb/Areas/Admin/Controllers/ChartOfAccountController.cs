using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Services;
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
    public class ChartOfAccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChartOfAccountController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public ChartOfAccountController(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<ChartOfAccountController> logger,
            IUnitOfWork unitOfWork,
            ICacheService cacheService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
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

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var level1 = await _unitOfWork.FilprideChartOfAccount
                .GetAllAsync(cancellationToken : cancellationToken);

            return View(level1.Where(c => c.Level == 1)
                .ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int parentId, string accountName, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var parentAccount = await _unitOfWork.FilprideChartOfAccount
                    .GetAsync(c => c.AccountId == parentId, cancellationToken);

                if (parentAccount == null)
                {
                    throw new InvalidOperationException("Parent Account not found");
                }

                var lastAccount = (await _unitOfWork.FilprideChartOfAccount
                        .GetAllAsync(c => c.ParentAccountId == parentId, cancellationToken: cancellationToken))
                    .OrderByDescending(c => c.AccountNumber)
                    .FirstOrDefault();

                var lastSeries = int.Parse(lastAccount?.AccountNumber ?? parentAccount.AccountNumber!);

                var levelToCreate = parentAccount.Level + 1;

                var newAccount = new ChartOfAccount
                {
                    IsMain = false,
                    AccountType = parentAccount?.AccountType,
                    NormalBalance = parentAccount?.NormalBalance ?? "",
                    AccountName = accountName,
                    ParentAccountId = parentId,
                    CreatedBy = GetUserFullName(),
                    Level = levelToCreate,
                    FinancialStatementType = parentAccount?.FinancialStatementType ?? "",
                };

                switch (levelToCreate)
                {
                    case 4:
                        newAccount.AccountNumber = (lastSeries + 100).ToString();
                        break;
                    case 5:
                        newAccount.AccountNumber = (lastSeries + 1).ToString();
                        break;
                }

                await _unitOfWork.FilprideChartOfAccount.AddAsync(newAccount, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
                await _cacheService.RemoveAsync($"coa:{await GetCompanyClaimAsync()}", cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (GetUserFullName(),
                    $"Created new Account #{newAccount.AccountNumber}", "Chart of Accounts", (await GetCompanyClaimAsync())! );
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Account #{newAccount.AccountNumber} Created Successfully";
                return Json(new { redirectUrl = Url.Action("Index", "ChartOfAccount", new { area = "Filpride" }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chart of account. Created by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int accountId, string accountName, CancellationToken cancellationToken)
        {
            var existingAccount = await _unitOfWork.FilprideChartOfAccount
                .GetAsync(x => x.AccountId == accountId, cancellationToken);

            if (existingAccount == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                existingAccount.AccountName = accountName;
                existingAccount.EditedBy = GetUserFullName();
                existingAccount.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
                await _unitOfWork.SaveAsync(cancellationToken);
                await _cacheService.RemoveAsync($"coa:{await GetCompanyClaimAsync()}", cancellationToken);

                #region --Audit Trail Recording

                AuditTrail auditTrailBook = new (GetUserFullName(),
                    $"Edited Account #{existingAccount.AccountNumber}", "Chart of Accounts", (await GetCompanyClaimAsync())! );
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Account Edited Successfully";
                return Json(new { redirectUrl = Url.Action("Index", "ChartOfAccount", new { area = "Filpride" }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit chart of account. Edited by: {UserName}", _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetChartOfAccountList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {
                var chartOfAccounts = _unitOfWork.FilprideChartOfAccount
                    .GetAllQuery();

                var totalRecords = await chartOfAccounts.CountAsync(cancellationToken);

                // Apply date range filter if provided (using CreatedDate)
                if (dateFrom.HasValue)
                {
                    chartOfAccounts = chartOfAccounts
                        .Where(s => s.CreatedDate >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    // Add one day to include the entire end date
                    var dateToInclusive = dateTo.Value.AddDays(1);
                    chartOfAccounts = chartOfAccounts
                        .Where(s => s.CreatedDate < dateToInclusive);
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasCreatedDate = DateTime.TryParse(searchValue, out var createdDate);

                    chartOfAccounts = chartOfAccounts
                        .Where(s =>
                            (s.AccountNumber != null && s.AccountNumber.ToLower().Contains(searchValue)) ||
                            (s.AccountName != null && s.AccountName.ToLower().Contains(searchValue)) ||
                            (s.AccountType != null && s.AccountType.ToLower().Contains(searchValue)) ||
                            (s.NormalBalance != null && s.NormalBalance.ToLower().Contains(searchValue)) ||
                            s.Level.ToString().Contains(searchValue) ||
                            (hasCreatedDate && DateOnly.FromDateTime(s.CreatedDate) == DateOnly.FromDateTime(createdDate))
                        );
                }

                //Apply sorting if provided
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    chartOfAccounts = chartOfAccounts
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await chartOfAccounts.CountAsync(cancellationToken);

                // Apply pagination - HANDLE -1 FOR "ALL"
                IQueryable<ChartOfAccount> pagedChartOfAccounts;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedChartOfAccounts = chartOfAccounts;
                }
                else
                {
                    // Normal pagination
                    pagedChartOfAccounts = chartOfAccounts
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = await pagedChartOfAccounts
                    .Select(x => new
                    {
                        x.AccountId,
                        x.AccountNumber,
                        x.AccountName,
                        x.AccountType,
                        x.NormalBalance,
                        x.Level,
                        x.CreatedDate
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
                _logger.LogError(ex, "Failed to get chart of accounts. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
