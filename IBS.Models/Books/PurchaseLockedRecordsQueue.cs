using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.AccountsPayable;

namespace IBS.Models.Books
{
    public class PurchaseLockedRecordsQueue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateOnly LockedDate { get; set; }

        public int ReceivingReportId { get; set; }

        [ForeignKey(nameof(ReceivingReportId))]
        public ReceivingReport ReceivingReport { get; set; } = null!;

        [Column(TypeName = "numeric(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal Price { get; set; }

    }
}
