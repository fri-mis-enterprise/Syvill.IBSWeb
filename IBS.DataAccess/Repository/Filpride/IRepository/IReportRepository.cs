using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.Common;
using IBS.Models.ViewModels;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IReportRepository : IRepository<GeneralLedgerBook>
    {
        Task<List<GeneralLedgerBook>> GetGeneralLedgerBooks(DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken = default);

        public Task<List<CheckVoucherHeader>> GetClearedDisbursementReport(DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken = default);

        public Task<List<ServiceInvoice>> GetServiceInvoiceReport(DateOnly dateFrom, DateOnly dateTo, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<ServiceInvoice>> GetARPerCustomerReport(DateOnly dateFrom, DateOnly dateTo, List<int>? customerIds = null, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<JournalVoucherDetail>> GetJournalVoucherReport(DateOnly dateFrom, DateOnly dateTo, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<CollectionReceipt>> GetCollectionReceiptReport(DateOnly dateFrom, DateOnly dateTo, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);
    }
}
