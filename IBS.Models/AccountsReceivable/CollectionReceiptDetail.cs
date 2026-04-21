using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.AccountsReceivable
{
    public class CollectionReceiptDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CollectionReceiptId { get; set; }

        public CollectionReceipt? CollectionReceipt { get; set; }

        [StringLength(13)]
        public string CollectionReceiptNo { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateOnly InvoiceDate { get; set; }

        [StringLength(13)]
        public string InvoiceNo { get; set; } = string.Empty;

        [Column(TypeName = "numeric(18,4)")]
        public decimal Amount { get; set; }
    }
}
