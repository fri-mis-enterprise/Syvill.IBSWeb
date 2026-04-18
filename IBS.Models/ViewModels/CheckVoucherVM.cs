using IBS.Models.AccountsPayable;
using IBS.Models.MasterFile;

namespace IBS.Models.ViewModels
{
    public class CheckVoucherVM
    {
        public FilprideCheckVoucherHeader? Header { get; set; }
        public List<FilprideCheckVoucherDetail>? Details { get; set; }

        public FilprideSupplier? Supplier { get; set; }

        public FilprideEmployee? Employee { get; set; }
    }
}
