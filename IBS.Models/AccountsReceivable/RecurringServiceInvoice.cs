using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Common;
using IBS.Models.MasterFile;

namespace IBS.Models.AccountsReceivable
{
    public class RecurringServiceInvoice: BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecurringServiceInvoiceId { get; set; }

        [Required]
        [StringLength(13)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Customer is required.")]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))] public Customer? Customer { get; set; }

        [Required(ErrorMessage = "The Service is required.")]
        [Display(Name = "Particulars")]
        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))] public Service? Service { get; set; }

        [StringLength(1000)]
        public string Instructions
        {
            get => _instructions;
            set => _instructions = value.Trim();
        }

        private string _instructions = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateOnly StartPeriod { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateOnly EndPeriod { get; set; }

        [Column(TypeName = "date")] public DateOnly? NextRunPeriod { get; set; }

        public int DurationInMonths { get; set; }

        public int GeneratedCount { get; set; }

        [Column(TypeName = "numeric(18,4)")] public decimal AmountPerMonth { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
