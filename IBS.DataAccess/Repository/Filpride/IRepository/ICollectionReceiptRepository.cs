using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICollectionReceiptRepository : IRepository<CollectionReceipt>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task UndoServiceInvoiceChanges(CollectionReceiptDetail collectionReceiptDetail, CancellationToken cancellationToken);

        Task RemoveSVPayment(int id, decimal paidAmount, CancellationToken cancellationToken = default);

        Task UpdateSV(int id, decimal paidAmount, CancellationToken cancellationToken = default);

        Task PostAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);

        Task DepositAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);

        Task ReturnedCheck(string crNo, string company, string userName, CancellationToken cancellationToken = default);

        Task RedepositAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);
    }
}
