using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IReceivingReportRepository : IRepository<ReceivingReport>
    {
        Task<string> GenerateCodeAsync(string company, string type, CancellationToken cancellationToken = default);

        Task<int> RemoveQuantityReceived(int id, decimal quantityReceived, CancellationToken cancellationToken = default);

        Task<string> AutoGenerateReceivingReport(DeliveryReceipt deliveryReceipt, DateOnly liftingDate, string userName, CancellationToken cancellationToken = default);

        Task PostAsync(ReceivingReport model, CancellationToken cancellationToken = default);

        Task VoidReceivingReportAsync(int receivingReportId, string currentUser, CancellationToken cancellationToken = default);

        Task CreateEntriesForUpdatingCost(ReceivingReport model, decimal difference, string userName, CancellationToken cancellationToken = default);

        Task UpdatePoAsync(int id, decimal quantityReceived, CancellationToken cancellationToken = default);
    }
}
