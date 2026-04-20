using IBS.Models.AccountsReceivable;

namespace IBS.Models.ViewModels
{
    public class SalesReportViewModel
    {
        public SalesInvoice? SalesInvoice { get; set; }
        public DeliveryReceipt DeliveryReceipt { get; set; } = null!;

        public string SalesInvoiceNo => SalesInvoice?.SalesInvoiceNo ?? " ";
    }
}
