using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IBS.Services
{
    [DisallowConcurrentExecution]
    public class DailyService : IJob
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly ILogger<DailyService> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        public DailyService(ApplicationDbContext dbContext, ILogger<DailyService> logger,
            UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTimeHelper.GetCurrentPhilippineTime());

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
