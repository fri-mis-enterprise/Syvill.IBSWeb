using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.MasterFile
{
    public class CustomerBranch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [StringLength(50)]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(200)]
        public string BranchAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string BranchTin { get; set; } = string.Empty;

        [NotMapped]
        public List<SelectListItem>? CustomerSelectList { get; set; }
    }
}
