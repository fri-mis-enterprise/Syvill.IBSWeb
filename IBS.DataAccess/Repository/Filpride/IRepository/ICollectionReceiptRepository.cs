using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.AccountsReceivable;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICollectionReceiptRepository : IRepository<CollectionReceipt>
    {
        Task<string> GenerateCodeAsync(string company, string type, CancellationToken cancellationToken = default);

        Task UpdateInvoice(int id, decimal paidAmount, CancellationToken cancellationToken = default);

        Task UndoSalesInvoiceChanges(CollectionReceiptDetail collectionReceiptDetail, CancellationToken cancellationToken);

        Task UndoServiceInvoiceChanges(CollectionReceiptDetail collectionReceiptDetail, CancellationToken cancellationToken);

        Task UpdateMultipleInvoice(string[] siNo, decimal[] paidAmount, CancellationToken cancellationToken = default);

        Task RemoveSIPayment(int id, decimal paidAmount, decimal offsetAmount, CancellationToken cancellationToken = default);

        Task RemoveSVPayment(int id, decimal paidAmount, decimal offsetAmount, CancellationToken cancellationToken = default);

        Task RemoveMultipleSIPayment(int[] id, decimal[] paidAmount, decimal offsetAmount, CancellationToken cancellationToken = default);

        Task UpdateSV(int id, decimal paidAmount, decimal offsetAmount, CancellationToken cancellationToken = default);

        Task PostAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);

        Task DepositAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);

        Task ReturnedCheck(string crNo, string company, string userName, CancellationToken cancellationToken = default);

        Task RedepositAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default);

        Task ApplyCostOfMoney(DeliveryReceipt deliveryReceipt, decimal costOfMoney, string currentUser, DateOnly depositedDate, CancellationToken cancellationToken = default);

        Task BatchPostCollectionAsync(CollectionReceipt collectionReceipt, List<AccountTitleDto> accountTitlesDto, CancellationToken cancellationToken = default);

        Task BatchDepositAsync(CollectionReceipt collectionReceipt, Dictionary<string, ChartOfAccount> accountTitlesDto,
            CancellationToken cancellationToken = default);
    }
}
