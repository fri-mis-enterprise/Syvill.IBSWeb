using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.AccountsReceivable
{
    public class DebitMemo: BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DebitMemoId { get; set; }

        [Display(Name = "SV No")] public int ServiceInvoiceId { get; set; }

        [ForeignKey(nameof(ServiceInvoiceId))] public ServiceInvoice? ServiceInvoice { get; set; }

        [NotMapped] public List<SelectListItem>? ServiceInvoices { get; set; }

        [StringLength(13)]
        [Display(Name = "DM No")]
        public string DebitMemoNo { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        [Display(Name = "Transaction Date")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateOnly TransactionDate { get; set; }

        [Display(Name = "Debit Amount")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal DebitAmount { get; set; }

        [StringLength(1000)]
        public string Description
        {
            get => _description;
            set => _description = value.Trim();
        }

        private string _description = string.Empty;

        [StringLength(20)] public string Source { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string? Remarks
        {
            get => _remarks;
            set => _remarks = value?.Trim();
        }

        private string? _remarks;

        [Column(TypeName = "date")] public DateOnly Period { get; set; }

        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal Amount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal CurrentAndPreviousAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal UnearnedAmount { get; set; }

        public bool IsPrinted { get; set; }

        [StringLength(50)] public string Status { get; set; } = nameof(Enums.Status.Pending);

        [StringLength(13)] public string? Type { get; set; }
    }
}
