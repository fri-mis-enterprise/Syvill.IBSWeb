using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IProvisionalReceiptRepository : IRepository<ProvisionalReceipt>
    {
        Task<string> GenerateSeriesNumberAsync(string type, CancellationToken cancellationToken = default);
        Task DepositAsync(ProvisionalReceipt provisionalReceipt, CancellationToken cancellationToken = default);
        Task ReturnedCheck(string prNo,string userName, CancellationToken cancellationToken = default);
    }
}
