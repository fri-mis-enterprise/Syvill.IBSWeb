using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.AccountsPayable
{
    public class CheckVoucherHeader : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CheckVoucherHeaderId { get; set; }

        [StringLength(13)]
        public string CheckVoucherHeaderNo { get; set; } = string.Empty;

        [Display(Name = "Transaction Date")]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateOnly Date { get; set; }

        [Display(Name = "SI No")]
        [Column(TypeName = "varchar[]")]
        public string[]? SINo { get; set; }

        [Display(Name = "PO No")]
        [Column(TypeName = "varchar[]")]
        public string[]? PONo { get; set; }

        [Display(Name = "Supplier Id")]
        public int? SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier? Supplier { get; set; }

        [StringLength(200)]
        public string? SupplierName { get; set; }

        [NotMapped]
        public List<SelectListItem>? Suppliers { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal Total { get; set; }

        [Column(TypeName = "numeric(18,4)[]")]
        public decimal[]? Amount { get; set; }

        [StringLength(1000)]
        public string? Particulars
        {
            get => _particulars;
            set => _particulars = value?.Trim();
        }

        private string? _particulars;

        [Display(Name = "Bank Account Name")]
        public int? BankId { get; set; }

        [ForeignKey(nameof(BankId))]
        public BankAccount? BankAccount { get; set; }

        [StringLength(200)]
        public string? BankAccountName { get; set; }

        [StringLength(100)]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Check #")]
        [StringLength(50)]
        public string? CheckNo
        {
            get => _checkNo;
            set => _checkNo = value?.Trim();
        }

        private string? _checkNo;

        [Display(Name = "Payee")]
        [StringLength(150)]
        public string Payee { get; set; } = string.Empty;

        [NotMapped]
        public List<SelectListItem>? BankAccounts { get; set; }

        [NotMapped]
        public List<SelectListItem>? COA { get; set; }

        [Display(Name = "Check Date")]
        [Column(TypeName = "date")]
        public DateOnly? CheckDate { get; set; }

        [StringLength(1000)]
        public string? Reference { get; set; }

        [NotMapped]
        public List<SelectListItem>? CheckVouchers { get; set; }

        [StringLength(10)]
        public string? CvType { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal CheckAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal AmountPaid { get; set; }

        public bool IsPaid { get; set; }

        public bool IsPrinted { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = nameof(CheckVoucherPaymentStatus.ForPosting);

        [StringLength(13)]
        public string Type { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal InvoiceAmount { get; set; }

        public string? SupportingFileSavedFileName { get; set; }

        public string? SupportingFileSavedUrl { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? DcpDate { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? DcrDate { get; set; }

        public bool IsAdvances { get; set; }

        public int? EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

        [StringLength(200)]
        public string Address { get; set; } = null!;

        [StringLength(20)]
        public string Tin { get; set; } = null!;

        public ICollection<CheckVoucherDetail>? Details { get; set; }

        [StringLength(20)]
        public string VatType { get; set; } = null!;

        [StringLength(20)]
        public string TaxType { get; set; } = null!;

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal TaxPercent { get; set; }

        public bool IsPayroll { get; set; }

        public DateOnly? LiquidationDate { get; set; }

        public string? ApprovedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ApprovedDate { get; set; }
    }
}
