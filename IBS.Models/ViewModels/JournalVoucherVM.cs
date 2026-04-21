using IBS.Models.AccountsPayable;

namespace IBS.Models.ViewModels
{
    public class JournalVoucherVM
    {
        public JournalVoucherHeader? Header { get; set; }
        public List<JournalVoucherDetail>? Details { get; set; }

        public bool IsAmortization { get; set; }
    }
}
