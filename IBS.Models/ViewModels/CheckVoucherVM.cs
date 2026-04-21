using IBS.Models.AccountsPayable;
using IBS.Models.MasterFile;

namespace IBS.Models.ViewModels
{
    public class CheckVoucherVM
    {
        public CheckVoucherHeader? Header { get; set; }
        public List<CheckVoucherDetail>? Details { get; set; }

        public Supplier? Supplier { get; set; }

        public Employee? Employee { get; set; }
    }
}
