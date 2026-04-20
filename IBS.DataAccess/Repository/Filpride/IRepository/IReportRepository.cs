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
        List<SalesBook> GetSalesBooks(DateOnly dateFrom, DateOnly dateTo, string? selectedDocument, string company);

        List<CashReceiptBook> GetCashReceiptBooks(DateOnly dateFrom, DateOnly dateTo, string company);

        List<PurchaseBook> GetPurchaseBooks(DateOnly dateFrom, DateOnly dateTo, string? selectedFiltering, string company);

        Task<List<ReceivingReport>> GetReceivingReportAsync(DateOnly? dateFrom, DateOnly? dateTo, string? selectedFiltering, string company, CancellationToken cancellationToken = default);

        List<Inventory> GetInventoryBooks(DateOnly dateFrom, DateOnly dateTo, string company);

        Task<List<GeneralLedgerBook>> GetGeneralLedgerBooks(DateOnly dateFrom, DateOnly dateTo, string company, CancellationToken cancellationToken = default);

        List<DisbursementBook> GetDisbursementBooks(DateOnly dateFrom, DateOnly dateTo, string company);

        List<JournalBook> GetJournalBooks(DateOnly dateFrom, DateOnly dateTo, string company);

        Task<List<AuditTrail>> GetAuditTrails(DateOnly dateFrom, DateOnly dateTo, string company);

        Task<List<CustomerOrderSlip>> GetCosUnservedVolume(DateOnly dateFrom, DateOnly dateTo, string company);

        public Task<List<SalesReportViewModel>> GetSalesReport(DateOnly dateFrom, DateOnly dateTo, string company, List<int>? commissioneeIds = null, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<SalesInvoice>> GetSalesInvoiceReport(DateOnly dateFrom, DateOnly dateTo, string company, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<FilpridePurchaseOrder>> GetPurchaseOrderReport(DateOnly dateFrom, DateOnly dateTo, string company, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<FilprideCheckVoucherHeader>> GetClearedDisbursementReport(DateOnly dateFrom, DateOnly dateTo, string company, CancellationToken cancellationToken = default);

        public Task<List<ReceivingReport>> GetPurchaseReport(DateOnly dateFrom, DateOnly dateTo, string company, List<int>? customerIds = null, List<int>? commissioneeIds = null, string dateSelectionType = "RRDate", string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<DeliveryReceipt>> GetGrossMarginReport( DateOnly dateFrom, DateOnly dateTo, string company, List<int>? customers = null, List<int>? commissionee = null, CancellationToken cancellationToken = default);
        public Task<List<CollectionReceipt>> GetCollectionReceiptReport(DateOnly dateFrom, DateOnly dateTo, string company, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<ReceivingReport>> GetTradePayableReport(DateOnly dateFrom, DateOnly dateTo, string company, CancellationToken cancellationToken = default);

        public Task<List<DeliveryReceipt>> GetHaulerPayableReport(DateOnly dateFrom, DateOnly dateTo, string company, CancellationToken cancellationToken = default);

        public Task<List<ServiceInvoice>> GetServiceInvoiceReport(DateOnly dateFrom, DateOnly dateTo, string company, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<FilpridePurchaseOrder>> GetApReport(DateOnly monthYear, string company, CancellationToken cancellationToken = default);

        public Task<List<SalesInvoice>> GetARPerCustomerReport(DateOnly dateFrom, DateOnly dateTo, string company, List<int>? customerIds = null, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);

        public Task<List<JournalVoucherDetail>> GetJournalVoucherReport(DateOnly dateFrom, DateOnly dateTo, string company, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default);
    }
}
