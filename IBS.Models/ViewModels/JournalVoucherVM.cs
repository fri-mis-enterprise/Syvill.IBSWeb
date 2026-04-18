using IBS.Models.AccountsPayable;

namespace IBS.Models.ViewModels
{
    public class JournalVoucherVM
    {
        public FilprideJournalVoucherHeader? Header { get; set; }
        public List<FilprideJournalVoucherDetail>? Details { get; set; }

        public bool IsAmortization { get; set; }
    }
}
