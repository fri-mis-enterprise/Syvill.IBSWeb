using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.MasterFile
{
    public class Terms
    {
        [Key] [StringLength(10)] public string TermsCode { get; set; } = string.Empty;

        public int NumberOfDays { get; set; }

        public int NumberOfMonths { get; set; }

        [StringLength(100)] public string CreatedBy { get; set; } = string.Empty;

        [Column(TypeName = "timestamp")] public DateTime CreatedDate { get; set; }

        [StringLength(100)] public string? EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }
    }
}
