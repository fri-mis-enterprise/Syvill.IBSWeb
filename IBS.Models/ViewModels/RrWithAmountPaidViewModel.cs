using IBS.Models.AccountsPayable;

namespace IBS.Models.ViewModels
{
    public class RrWithAmountPaidViewModel
    {
        public ReceivingReport ReceivingReport { get; set; } = null!;
        public decimal AmountPaid { get; set; }
    }
}
