using System.ComponentModel.DataAnnotations;
using IBS.Models.Enums;

namespace IBS.Models.ViewModels
{
    public class PRCreateViewModel: ProvisionalReceiptViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string Type { get; set; } = nameof(DocumentType.Documented);
    }
}
