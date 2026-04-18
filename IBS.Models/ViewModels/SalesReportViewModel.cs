using IBS.Models.AccountsReceivable;

namespace IBS.Models.ViewModels
{
    public class SalesReportViewModel
    {
        public FilprideSalesInvoice? SalesInvoice { get; set; }
        public FilprideDeliveryReceipt DeliveryReceipt { get; set; } = null!;

        public string SalesInvoiceNo => SalesInvoice?.SalesInvoiceNo ?? " ";
    }
}
