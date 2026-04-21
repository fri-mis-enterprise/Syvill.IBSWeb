using System.ComponentModel.DataAnnotations;

namespace IBS.Models.MasterFile
{
    public class Terms
    {
        [Key]
        [StringLength(10)]
        public string TermsCode { get; set; } = null!;

        public int NumberOfDays { get; set; }

        public int NumberOfMonths { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        public string? EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }
    }
}
