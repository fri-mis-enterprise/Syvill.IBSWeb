using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Books;
using IBS.Models.Common;
using IBS.Models.MasterFile;

namespace IBS.Models.AccountsReceivable
{
    public class CollectionReceipt: BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollectionReceiptId { get; set; }

        [StringLength(13)]
        [Display(Name = "CR No")]
        public string CollectionReceiptNo { get; set; } = string.Empty;

        public int ServiceInvoiceId { get; set; }

        [Display(Name = "Service Invoice No.")]
        [StringLength(13)]
        public string? SVNo { get; set; }

        [ForeignKey(nameof(ServiceInvoiceId))] public ServiceInvoice? ServiceInvoice { get; set; }

        [ForeignKey(nameof(CustomerId))] public Customer? Customer { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateOnly TransactionDate { get; set; }

        [Display(Name = "Reference No")]
        [Required]
        [StringLength(50)]
        public string ReferenceNo
        {
            get => _referenceNo;
            set => _referenceNo = value.Trim();
        }

        private string _referenceNo = null!;

        [StringLength(100)]
        public string? Remarks
        {
            get => _remarks;
            set => _remarks = value?.Trim();
        }

        private string? _remarks;

        //Cash
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal CashAmount { get; set; }

        //Check
        [Column(TypeName = "date")] public DateOnly? CheckDate { get; set; }

        [StringLength(50)]
        public string? CheckNo
        {
            get => _checkNo;
            set => _checkNo = value?.Trim();
        }

        private string? _checkNo;

        [StringLength(50)]
        public string? CheckBank
        {
            get => _checkBank;
            set => _checkBank = value?.Trim();
        }

        private string? _checkBank;

        [StringLength(50)]
        public string? CheckBranch
        {
            get => _checkBranch;
            set => _checkBranch = value?.Trim();
        }

        private string? _checkBranch;

        //Check
        [Column(TypeName = "date")] public DateOnly? ManagersCheckDate { get; set; }

        [StringLength(50)]
        public string? ManagersCheckNo
        {
            get => _managersCheckNo;
            set => _managersCheckNo = value?.Trim();
        }

        private string? _managersCheckNo;

        [StringLength(50)]
        public string? ManagersCheckBank
        {
            get => _managersCheckBank;
            set => _managersCheckBank = value?.Trim();
        }

        private string? _managersCheckBank;

        [StringLength(50)]
        public string? ManagersCheckBranch
        {
            get => _managersCheckBranch;
            set => _managersCheckBranch = value?.Trim();
        }

        private string? _managersCheckBranch;

        public int? BankId { get; set; }

        [ForeignKey(nameof(BankId))] public BankAccount? BankAccount { get; set; }

        [StringLength(50)] public string? BankAccountName { get; set; }

        [StringLength(30)] public string? BankAccountNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal CheckAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal ManagersCheckAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal EWT { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal WVAT { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal Total { get; set; }

        public bool IsCertificateUpload { get; set; }

        [StringLength(200)] public string? F2306FilePath { get; set; }

        [StringLength(100)] public string? F2306FileName { get; set; }

        [StringLength(200)] public string? F2307FilePath { get; set; }

        [StringLength(100)] public string? F2307FileName { get; set; }

        public bool IsPrinted { get; set; }

        public DateOnly[]? MultipleTransactionDate { get; set; }

        [StringLength(50)] public string Status { get; set; } = nameof(Enums.Status.Pending);

        [StringLength(13)] public string? Type { get; set; }

        [Column(TypeName = "date")] public DateOnly? DepositedDate { get; set; }

        [NotMapped] public List<GeneralLedgerBook>? Details { get; set; }

        public ICollection<CollectionReceiptDetail>? ReceiptDetails { get; set; }

        [Column(TypeName = "date")] public DateOnly? ClearedDate { get; set; }

        [StringLength(50)] public string BatchNumber { get; set; } = string.Empty;
    }
}
