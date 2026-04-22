using IBS.DataAccess.Data;
using IBS.Models;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using IBS.Models.MasterFile;
using IBS.Models.ViewModels;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    public class HomeController: Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        private async Task<string?> GetCompanyClaimAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return string.Empty;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == "Company")?.Value;
        }

        public async Task<IActionResult> Index()
        {
            var findUser = await _dbContext.ApplicationUsers
                .Where(user => user.Id == _userManager.GetUserId(this.User))
                .FirstOrDefaultAsync();

            ViewBag.GetUserDepartment = findUser?.Department;
            var companyClaims = findUser != null ? await GetCompanyClaimAsync() : string.Empty;

            var dashboardCounts = new DashboardCountViewModel
            {
                #region -- Accounting - For Approval

                JournalVoucherForApprovalCount = await _dbContext.JournalVoucherHeaders
                    .Where(jv => jv.Status == nameof(JvStatus.ForApproval))
                    .CountAsync(),
                CheckVoucherNonTradeInvoiceForApprovalCount = await _dbContext.CheckVoucherHeaders
                    .Where(cv => cv.Status == nameof(CheckVoucherInvoiceStatus.ForApproval)
                                 && cv.CvType == nameof(CVType.Invoicing)
                                 && !cv.IsPayroll)
                    .CountAsync(),
                CheckVoucherNonTradePayrollInvoiceForApprovalCount = await _dbContext.CheckVoucherHeaders
                    .Where(cv => cv.Status == nameof(CheckVoucherInvoiceStatus.ForApproval)
                                 && cv.CvType == nameof(CVType.Invoicing)
                                 && cv.IsPayroll)
                    .CountAsync(),

                #endregion -- Accounting - For Approval
            };

            return View(dashboardCounts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Maintenance()
        {
            if (await _dbContext.AppSettings
                    .Where(s => s.SettingKey == "MaintenanceMode")
                    .Select(s => s.Value == "true")
                    .FirstOrDefaultAsync())
            {
                return View("Maintenance");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
