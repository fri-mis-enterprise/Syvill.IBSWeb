using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IBS.Services
{
    [DisallowConcurrentExecution]
    public class StartOfTheMonthService: IJob
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<StartOfTheMonthService> _logger;

        private readonly ApplicationDbContext _dbContext;

        private readonly IServiceInvoiceGenerationService _serviceInvoiceGenerationService;

        public StartOfTheMonthService(IUnitOfWork unitOfWork,
            ILogger<StartOfTheMonthService> logger, ApplicationDbContext dbContext,
            IServiceInvoiceGenerationService serviceInvoiceGenerationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dbContext = dbContext;
            _serviceInvoiceGenerationService = serviceInvoiceGenerationService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTimeHelper.GetCurrentPhilippineTime());
                var currentPeriod = new DateOnly(today.Year, today.Month, 1);

                await ProcessAmortization(today);
                await ProcessRecurringServiceInvoices(currentPeriod);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ProcessAmortization(DateOnly dateToday)
        {
            try
            {
                var amortizationSetting = await _dbContext.JvAmortizationSettings
                    .Include(x => x.JvHeader)
                    .ThenInclude(x => x.Details)
                    .Where(x =>
                        (x.NextRunDate == null || x.NextRunDate <= dateToday) &&
                        x.IsActive &&
                        x.JvHeader.PostedBy != null)
                    .ToListAsync();

                if (amortizationSetting.Count == 0)
                {
                    return;
                }

                var newJournalVouchers = new List<JournalVoucherHeader>();

                var groupedAmortizations = amortizationSetting
                    .GroupBy(a => new { a.JvHeader.Type })
                    .ToList();

                foreach (var group in groupedAmortizations)
                {
                    var baseCode = await _unitOfWork.JournalVoucher
                        .GenerateCodeAsync(group.Key.Type);

                    var offset = 0;
                    foreach (var amortization in group)
                    {
                        var sourceJv = amortization.JvHeader;

                        if (sourceJv?.Details == null || sourceJv.Details.Count == 0)
                        {
                            throw new InvalidOperationException(
                                $"The source journal voucher for amortization with ID {amortization.JvId} is missing or has no details.");
                        }

                        var generatedCode = IncrementCode(baseCode, offset++);

                        var newHeader = new JournalVoucherHeader
                        {
                            Type = sourceJv.Type,
                            JournalVoucherHeaderNo = generatedCode,
                            Date = dateToday,
                            References = sourceJv.References,
                            CVId = sourceJv.CVId,
                            Particulars = sourceJv.Particulars,
                            CRNo = sourceJv.CRNo,
                            JVReason = sourceJv.JVReason,
                            CreatedBy = "SYSTEM",
                            JvType = nameof(JvType.Amortization),
                            Status = nameof(JvStatus.Pending),
                            Details = sourceJv.Details.Select(detail => new JournalVoucherDetail
                            {
                                AccountNo = detail.AccountNo,
                                AccountName = detail.AccountName,
                                TransactionNo = detail.TransactionNo,
                                Debit = detail.Debit,
                                Credit = detail.Credit,
                                SubAccountType = detail.SubAccountType,
                                SubAccountId = detail.SubAccountId,
                                SubAccountName = detail.SubAccountName
                            }).ToList()
                        };

                        newJournalVouchers.Add(newHeader);
                        amortization.LastRunDate = dateToday;
                        if (amortization.OccurrenceRemaining > 0)
                        {
                            amortization.OccurrenceRemaining--;
                        }

                        amortization.IsActive = amortization.OccurrenceRemaining > 0;
                        amortization.NextRunDate = amortization.IsActive ? dateToday.AddMonths(1) : null;
                    }
                }

                await _dbContext.JournalVoucherHeaders.AddRangeAsync(newJournalVouchers);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing amortization for {Date}", dateToday);
                throw;
            }
        }

        private async Task ProcessRecurringServiceInvoices(DateOnly currentPeriod)
        {
            try
            {
                var recurringInvoices = await _dbContext.RecurringServiceInvoices
                    .Where(rsi =>
                        rsi.IsActive &&
                        rsi.NextRunPeriod != null &&
                        rsi.NextRunPeriod <= currentPeriod)
                    .OrderBy(rsi => rsi.NextRunPeriod)
                    .ToListAsync();

                if (recurringInvoices.Count == 0)
                {
                    return;
                }

                foreach (var recurringInvoice in recurringInvoices)
                {
                    while (recurringInvoice.IsActive &&
                           recurringInvoice.NextRunPeriod != null &&
                           recurringInvoice.NextRunPeriod <= currentPeriod)
                    {
                        var invoicePeriod = NormalizePeriod(recurringInvoice.NextRunPeriod.Value);

                        if (!await HasGeneratedInvoiceAsync(recurringInvoice.RecurringServiceInvoiceId, invoicePeriod))
                        {
                            var generatedInvoice = await _serviceInvoiceGenerationService.CreateAsync(
                                new ServiceInvoiceGenerationRequest
                                {
                                    Type = recurringInvoice.Type,
                                    CustomerId = recurringInvoice.CustomerId,
                                    ServiceId = recurringInvoice.ServiceId,
                                    Period = invoicePeriod,
                                    DueDate = invoicePeriod.AddMonths(1).AddDays(-1),
                                    Instructions = recurringInvoice.Instructions,
                                    Total = recurringInvoice.AmountPerMonth,
                                    Discount = 0,
                                    CreatedBy = "SYSTEM",
                                    RecurringServiceInvoiceId = recurringInvoice.RecurringServiceInvoiceId
                                });

                            await _unitOfWork.AuditTrail.AddAsync(new AuditTrail("SYSTEM",
                                $"Generated service invoice# {generatedInvoice.ServiceInvoiceNo} from recurring setup# {recurringInvoice.RecurringServiceInvoiceId}",
                                "Service Invoice"));
                        }

                        recurringInvoice.GeneratedCount =
                            Math.Max(recurringInvoice.GeneratedCount, GetSequenceNumber(recurringInvoice, invoicePeriod));
                        recurringInvoice.IsActive = recurringInvoice.GeneratedCount < recurringInvoice.DurationInMonths;
                        recurringInvoice.NextRunPeriod = recurringInvoice.IsActive
                            ? recurringInvoice.StartPeriod.AddMonths(recurringInvoice.GeneratedCount)
                            : null;
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing recurring service invoices for {Period}", currentPeriod);
                throw;
            }
        }

        private async Task<bool> HasGeneratedInvoiceAsync(int recurringServiceInvoiceId, DateOnly invoicePeriod)
        {
            return await _dbContext.ServiceInvoices
                .AnyAsync(sv =>
                    sv.RecurringServiceInvoiceId == recurringServiceInvoiceId &&
                    sv.Period == invoicePeriod &&
                    sv.Status != nameof(Status.Voided));
        }

        private static int GetSequenceNumber(RecurringServiceInvoice recurringInvoice, DateOnly invoicePeriod)
        {
            return ((invoicePeriod.Year - recurringInvoice.StartPeriod.Year) * 12) +
                   invoicePeriod.Month - recurringInvoice.StartPeriod.Month + 1;
        }

        private static DateOnly NormalizePeriod(DateOnly period)
        {
            return new DateOnly(period.Year, period.Month, 1);
        }

        private static string IncrementCode(string baseCode, int offset)
        {
            if (baseCode.StartsWith("JVU"))
            {
                var numericPart = baseCode.Substring(3);
                var incremented = long.Parse(numericPart) + offset;
                return "JVU" + incremented.ToString("D9");
            }
            else
            {
                var numericPart = baseCode.Substring(2);
                var incremented = long.Parse(numericPart) + offset;
                return "JV" + incremented.ToString("D10");
            }
        }
    }
}
