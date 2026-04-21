using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.Common;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<LogMessage> LogMessages { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<HubConnection> HubConnections { get; set; }

        public DbSet<PostedPeriod> PostedPeriods { get; set; }

        public DbSet<CustomerBranch> CustomerBranches { get; set; }

        public DbSet<MonthlyNibit> MonthlyNibits { get; set; }

        public DbSet<GLPeriodBalance> GlPeriodBalances { get; set; }

        public DbSet<GLSubAccountBalance> GlSubAccountBalances { get; set; }

        public DbSet<ProvisionalReceipt> ProvisionalReceipts { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Terms> Terms { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
        public DbSet<Product> Products { get; set; }

        #region AAS Migration

        #region Accounts Receivable
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<CollectionReceipt> CollectionReceipts { get; set; }
        public DbSet<CreditMemo> CreditMemos { get; set; }
        public DbSet<DebitMemo> DebitMemos { get; set; }
        public DbSet<ServiceInvoice> ServiceInvoices { get; set; }
        public DbSet<CollectionReceiptDetail> CollectionReceiptDetails { get; set; }
        #endregion

        #region Accounts Payable

        public DbSet<CheckVoucherHeader> CheckVoucherHeaders { get; set; }
        public DbSet<CheckVoucherDetail> CheckVoucherDetails { get; set; }
        public DbSet<JournalVoucherHeader> JournalVoucherHeaders { get; set; }
        public DbSet<JournalVoucherDetail> JournalVoucherDetails { get; set; }

        public DbSet<MultipleCheckVoucherPayment> MultipleCheckVoucherPayments { get; set; }

        public DbSet<JvAmortizationSetting> JvAmortizationSettings { get; set; }

        #endregion

        #region Books
        public DbSet<GeneralLedgerBook> GeneralLedgerBooks { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }

        #endregion

        #endregion

        #region--Fluent API Implementation

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region-- Master File

            // Company
            builder.Entity<Company>(c =>
            {
                c.HasIndex(c => c.CompanyCode).IsUnique();
                c.HasIndex(c => c.CompanyName).IsUnique();
            });

            // Product
            builder.Entity<Product>(p =>
            {
                p.HasIndex(p => p.ProductCode).IsUnique();
                p.HasIndex(p => p.ProductName).IsUnique();
            });

            #endregion

            #region--Chart Of Account
            builder.Entity<ChartOfAccount>(coa =>
            {
                coa.HasIndex(coa => coa.AccountNumber).IsUnique();
                coa.HasIndex(coa => coa.AccountName);
            });
            #endregion

            #region--Filpride

            builder.Entity<CustomerBranch>(b =>
            {
                b.HasOne(b => b.Customer)
                    .WithMany(c => c.Branches)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<MonthlyNibit>(n =>
            {
                n.HasQueryFilter(x => x.IsValid);
                n.HasIndex(n => n.Month);
                n.HasIndex(n => n.Year);
            });

            #region-- Master File

            // FilprideCustomer
            builder.Entity<Customer>(c =>
            {
                c.HasIndex(c => c.CustomerCode);
                c.HasIndex(c => c.CustomerName);
            });

            // FilprideSupplier
            builder.Entity<Supplier>(s =>
            {
                s.HasIndex(s => s.SupplierCode);
                s.HasIndex(s => s.SupplierName);
            });

            // FilprideEmployee
            builder.Entity<Employee>(c =>
            {
                c.HasIndex(c => c.EmployeeNumber);
            });

            builder.Entity<GLPeriodBalance>(b =>
            {
                b.HasQueryFilter(x => x.IsValid);

                b.HasOne(a => a.Account)
                    .WithMany(c => c.Balances)
                    .HasForeignKey(a => a.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<GLSubAccountBalance>(b =>
            {
                b.HasQueryFilter(x => x.IsValid);

                b.HasOne(a => a.Account)
                    .WithMany(c => c.SubAccountBalances)
                    .HasForeignKey(a => a.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region -- Accounts Receivable --

            #region -- Service Invoice --

            builder.Entity<ServiceInvoice>(sv =>
            {
                sv.HasOne(sv => sv.Customer)
                .WithMany()
                .HasForeignKey(sv => sv.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

                sv.HasOne(sv => sv.Service)
                .WithMany()
                .HasForeignKey(sv => sv.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

                sv.HasIndex(sv => new
                {
                    sv.ServiceInvoiceNo
                })
                .IsUnique();
            });

            #endregion -- Service Invoice --

            #region -- Collection Receipt --

            builder.Entity<CollectionReceipt>(cr =>
            {
                cr.HasOne(cr => cr.ServiceInvoice)
                .WithMany()
                .HasForeignKey(cr => cr.ServiceInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

                cr.HasOne(cr => cr.Customer)
                .WithMany()
                .HasForeignKey(cr => cr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

                cr.HasOne(cr => cr.BankAccount)
                    .WithMany()
                    .HasForeignKey(cr => cr.BankId)
                    .OnDelete(DeleteBehavior.Restrict);

                cr.HasIndex(cr => new
                {
                    cr.CollectionReceiptNo
                })
                .IsUnique();
            });

            builder.Entity<CollectionReceiptDetail>(crd =>
            {
                crd.HasOne(d => d.CollectionReceipt)
                    .WithMany(d => d.ReceiptDetails)
                    .HasForeignKey(d => d.CollectionReceiptId)
                    .OnDelete(DeleteBehavior.Restrict);

                crd.HasIndex(d => d.InvoiceNo);

                crd.HasIndex(d => d.CollectionReceiptNo);
            });

            builder.Entity<ProvisionalReceipt>(pr =>
            {
                pr.HasOne(p => p.Employee)
                    .WithMany()
                    .HasForeignKey(p => p.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                pr.HasOne(p => p.BankAccount)
                    .WithMany()
                    .HasForeignKey(p => p.BankId)
                    .OnDelete(DeleteBehavior.Restrict);

                pr.HasIndex(d => new
                {
                    d.SeriesNumber
                }).IsUnique();
            });

            #endregion -- Collection Receipt --

            #region -- Debit Memo --

            builder.Entity<DebitMemo>(dm =>
            {
                dm.HasOne(dm => dm.ServiceInvoice)
                .WithMany()
                .HasForeignKey(dm => dm.ServiceInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

                dm.HasIndex(dm => new
                {
                    dm.DebitMemoNo
                })
                .IsUnique();
            });

            #endregion -- Debit Memo --

            #region -- Credit Memo --

            builder.Entity<CreditMemo>(cm =>
            {
                cm.HasOne(cm => cm.ServiceInvoice)
                .WithMany()
                .HasForeignKey(cm => cm.ServiceInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

                cm.HasIndex(cm => new
                {
                    cm.CreditMemoNo
                })
                .IsUnique();
            });

            #endregion -- Credit Memo --

            #endregion -- Accounts Receivable --

            #region -- Accounts Payable --

            #region -- Check Voucher Header --

            builder.Entity<CheckVoucherHeader>(cv =>
            {
                cv.HasOne(cv => cv.Supplier)
                .WithMany()
                .HasForeignKey(cv => cv.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

                cv.HasOne(cv => cv.BankAccount)
                .WithMany()
                .HasForeignKey(cv => cv.BankId)
                .OnDelete(DeleteBehavior.Restrict);

                cv.HasIndex(cv => new
                {
                    cv.CheckVoucherHeaderNo
                })
                .IsUnique();
            });

            #endregion -- Check Voucher --

            #region -- Check Voucher Details --

            builder.Entity<CheckVoucherDetail>(cv =>
            {
                cv.HasOne(cv => cv.CheckVoucherHeader)
                    .WithMany(cv => cv.Details)
                    .HasForeignKey(cv => cv.CheckVoucherHeaderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion -- Check Voucher Details --

            #region -- Journal Voucher Header --

            builder.Entity<JournalVoucherHeader>(jv =>
            {
                jv.HasOne(jv => jv.CheckVoucherHeader)
                .WithMany()
                .HasForeignKey(jv => jv.CVId)
                .OnDelete(DeleteBehavior.Restrict);

                jv.HasIndex(jv => new
                {
                    jv.JournalVoucherHeaderNo
                })
                .IsUnique();
            });

            #endregion -- Check Voucher --

            #region -- Journal Voucher Details --

            builder.Entity<JournalVoucherDetail>(jv =>
            {
                jv.HasOne(jv => jv.JournalVoucherHeader)
                    .WithMany(jv => jv.Details)
                    .HasForeignKey(jv => jv.JournalVoucherHeaderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region

            builder.Entity<JvAmortizationSetting>(jv =>
            {
                jv.HasOne(jv => jv.JvHeader)
                    .WithMany()
                    .HasForeignKey(jv => jv.JvId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region -- Multiple Check Voucher Payment --

            builder.Entity<MultipleCheckVoucherPayment>(mcvp =>
            {
                mcvp.HasOne(mcvp => mcvp.CheckVoucherHeaderPayment)
                    .WithMany()
                    .HasForeignKey(mcvp => mcvp.CheckVoucherHeaderPaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                mcvp.HasOne(mcvp => mcvp.CheckVoucherHeaderInvoice)
                    .WithMany()
                    .HasForeignKey(mcvp => mcvp.CheckVoucherHeaderInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion -- Multiple Check Voucher Payment --

            #endregion -- Accounts Payable --

            #region-- Books --

            builder.Entity<GeneralLedgerBook>(gl =>
            {
                gl.HasOne(gl => gl.Account)
                    .WithMany()
                    .HasForeignKey(gl => gl.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #endregion

            #region--AppSettings

            builder.Entity<AppSetting>(a =>
            {
                a.HasIndex(a => a.SettingKey).IsUnique();
            });

            #endregion
        }

        #endregion
    }
}
