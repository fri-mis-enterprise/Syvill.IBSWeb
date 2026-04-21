using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DataAccess.Repository.IRepository;
using IBS.DataAccess.Repository.MasterFile;
using IBS.DataAccess.Repository.MasterFile.IRepository;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Expressions;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }

        public INotificationRepository Notifications { get; private set; }

        public async Task<bool> IsPeriodPostedAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _db.PostedPeriods
                .AnyAsync(m => m.IsPosted
                               && m.Month == date.Month
                               && m.Year == date.Year, cancellationToken);
        }

        public async Task<DateTime> GetMinimumPeriodBasedOnThePostedPeriods(Module module, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(Module), module))
            {
                throw new InvalidEnumArgumentException(nameof(module), (int)module, typeof(Module));
            }

            var period = await _db.PostedPeriods
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .FirstOrDefaultAsync(x => x.Module == module.ToString()
                                          && x.IsPosted, cancellationToken);

            if (period == null)
            {
                return DateTime.MinValue;
            }

            return new DateOnly(period.Year, period.Month, 1)
                .AddMonths(1)
                .ToDateTime(new TimeOnly(0, 0));
        }

        public async Task<bool> IsPeriodPostedAsync(Module module, DateOnly date, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(Module), module))
            {
                throw new InvalidEnumArgumentException(nameof(module), (int)module, typeof(Module));
            }

            return await _db.PostedPeriods
                .AnyAsync(m =>
                    m.Module == module.ToString() &&
                    m.IsPosted &&
                    m.Year == date.Year &&
                    m.Month == date.Month,
                    cancellationToken);
        }

        #region--Filpride
        public Filpride.IRepository.ICustomerRepository Customer { get; private set; }
        public Filpride.IRepository.ISupplierRepository Supplier { get; private set; }
        public Filpride.IRepository.IChartOfAccountRepository ChartOfAccount { get; private set; }
        public IAuditTrailRepository AuditTrail { get; private set; }
        public Filpride.IRepository.IEmployeeRepository Employee { get; private set; }
        public ICustomerBranchRepository CustomerBranch { get; private set; }
        public ITermsRepository Terms { get; private set; }
        public Filpride.IRepository.IGeneralLedgerRepository GeneralLedger { get; private set; }
        public IProvisionalReceiptRepository ProvisionalReceipt { get; private set; }

        #endregion

        #region AAS

        #region Accounts Receivable

        public Filpride.IRepository.IServiceInvoiceRepository ServiceInvoice { get; private set; }

        public Filpride.IRepository.ICollectionReceiptRepository CollectionReceipt { get; private set; }

        public Filpride.IRepository.IDebitMemoRepository DebitMemo { get; private set; }

        public Filpride.IRepository.ICreditMemoRepository CreditMemo { get; private set; }
        #endregion

        #region Accounts Payable
        public Filpride.IRepository.ICheckVoucherRepository CheckVoucher { get; private set; }

        public Filpride.IRepository.IJournalVoucherRepository JournalVoucher { get; private set; }
        #endregion

        #region Books and Report
        public IReportRepository Report { get; private set; }
        #endregion

        #region Master File

        public Filpride.IRepository.IBankAccountRepository BankAccount { get; private set; }

        public Filpride.IRepository.IServiceRepository Service { get; private set; }

        #endregion

        #endregion

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            Notifications = new NotificationRepository(_db);

            #region--Filpride

            Customer = new CustomerRepository(_db);
            Supplier = new SupplierRepository(_db);
            ChartOfAccount = new ChartOfAccountRepository(_db);
            AuditTrail = new AuditTrailRepository(_db);
            Employee = new EmployeeRepository(_db);
            CustomerBranch = new CustomerBranchRepository(_db);
            Terms = new TermsRepository(_db);
            GeneralLedger = new GeneralLedgerRepository(_db);
            ProvisionalReceipt = new ProvisionalReceiptRepository(_db);

            #endregion

            #region AAS

            #region Accounts Receivable
            ServiceInvoice = new Filpride.ServiceInvoiceRepository(_db);
            CollectionReceipt = new Filpride.CollectionReceiptRepository(_db);
            DebitMemo = new Filpride.DebitMemoRepository(_db);
            CreditMemo = new Filpride.CreditMemoRepository(_db);
            #endregion

            #region Accounts Payable
            CheckVoucher = new Filpride.CheckVoucherRepository(_db);
            JournalVoucher = new Filpride.JournalVoucherRepository(_db);
            #endregion

            #region Books and Report
            Report = new ReportRepository(_db);
            #endregion

            #region Master File

            BankAccount = new Filpride.BankAccountRepository(_db);
            Service = new Filpride.ServiceRepository(_db);

            #endregion

            #endregion
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        public void Dispose() => _db.Dispose();

        #region--Filpride

        public async Task<List<SelectListItem>> GetCustomerListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Customers
                .OrderBy(c => c.CustomerName)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Suppliers
                .OrderBy(s => s.SupplierCode)
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierCode + " " + s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetNonTradeSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Suppliers
                .OrderBy(s => s.SupplierName)
                .Where(s => s.IsActive && s.Category == "Non-Trade")
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetBankAccountListById(CancellationToken cancellationToken = default)
        {
            return await _db.BankAccounts
                .OrderBy(b => b.AccountNo)
                .Select(ba => new SelectListItem
                {
                    Value = ba.BankAccountId.ToString(),
                    Text = ba.Bank + " " + ba.AccountNo + " " + ba.AccountName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetEmployeeListById(CancellationToken cancellationToken = default)
        {
            return await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = $"{e.EmployeeNumber} - {e.FirstName} {e.LastName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetServiceListById( CancellationToken cancellationToken = default)
        {
            return await _db.Services
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = s.Name
                })
                .ToListAsync(cancellationToken);
        }

        #endregion

        public async Task<List<SelectListItem>> GetProductListAsyncByCode(CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .OrderBy(p => p.ProductCode)
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductCode,
                    Text = p.ProductCode + " " + p.ProductName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetProductListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .OrderBy(p => p.ProductCode)
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductCode + " " + p.ProductName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountId.ToString(),
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncByNo(CancellationToken cancellationToken = default)
        {
            return await _db.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber,
                    Text = $"({s.AccountType}) {s.AccountNumber} {s.AccountName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncByAccountTitle(CancellationToken cancellationToken = default)
        {
            return await _db.ChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber + " " + s.AccountName,
                    Text = $"({s.AccountType}) {s.AccountNumber} {s.AccountName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetCompanyListAsyncByName(CancellationToken cancellationToken = default)
        {
            return await _db.Companies
                .OrderBy(c => c.CompanyCode)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CompanyName,
                    Text = c.CompanyCode + " " + c.CompanyName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetCompanyListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Companies
                .OrderBy(c => c.CompanyCode)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CompanyId.ToString(),
                    Text = c.CompanyCode + " " + c.CompanyName
                })
                .ToListAsync(cancellationToken);
        }
    }
}
