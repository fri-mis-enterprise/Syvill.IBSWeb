using IBS.Models.AccountsPayable;

namespace IBS.Models.ViewModels
{
    public class RrWithAmountPaidViewModel
    {
        public FilprideReceivingReport ReceivingReport { get; set; } = null!;
        public decimal AmountPaid { get; set; }
    }
}
