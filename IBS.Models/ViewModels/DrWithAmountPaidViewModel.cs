using IBS.Models.AccountsReceivable;

namespace IBS.Models.ViewModels
{
    public class DrWithAmountPaidViewModel
    {
        public FilprideDeliveryReceipt DeliveryReceipt { get; set; } = null!;
        public decimal AmountPaid { get; set; }
    }

    public record MonthYear(int Year, int Month);
}
