using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.AccountsPayable
{
    public class CVTradePayment
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }

        [StringLength(5)]
        public string DocumentType { get; set; } = null!;

        public int CheckVoucherId { get; set; }

        [ForeignKey(nameof(CheckVoucherId))]
        public FilprideCheckVoucherHeader CV { get; set; } = null!;

        [Column(TypeName = "numeric(18,4)")]
        public decimal AmountPaid { get; set; }
    }
}
