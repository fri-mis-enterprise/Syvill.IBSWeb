using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DataAccess.Repository.MasterFile.IRepository;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Product { get; }

        ICompanyRepository Company { get; }

        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetProductListAsyncByCode(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetProductListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncByNo(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncByAccountTitle(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetCompanyListAsyncByName(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetCompanyListAsyncById(CancellationToken cancellationToken = default);

        #region--Filpride

        IChartOfAccountRepository ChartOfAccount { get; }
        ISupplierRepository Supplier { get; }
        ICustomerRepository Customer { get; }
        IAuditTrailRepository AuditTrail { get; }
        IEmployeeRepository Employee { get; }
        ICustomerBranchRepository CustomerBranch { get; }
        ITermsRepository Terms { get; }
        IGeneralLedgerRepository GeneralLedger { get; }
        IProvisionalReceiptRepository ProvisionalReceipt { get; }

        Task<List<SelectListItem>> GetCustomerListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetNonTradeSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetBankAccountListById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetEmployeeListById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetServiceListById(CancellationToken cancellationToken = default);

        #endregion

        #region AAS

        #region Accounts Receivable

        IServiceInvoiceRepository ServiceInvoice { get; }

        ICollectionReceiptRepository CollectionReceipt { get; }

        IDebitMemoRepository DebitMemo { get; }

        ICreditMemoRepository CreditMemo { get; }
        #endregion

        #region Accounts Payable

        ICheckVoucherRepository CheckVoucher { get; }

        IJournalVoucherRepository JournalVoucher { get; }

        #endregion

        #region Books and Report

        IReportRepository Report { get; }
        #endregion

        #region Master File

        IBankAccountRepository BankAccount { get; }

        IServiceRepository Service { get; }

        #endregion

        #endregion

        INotificationRepository Notifications { get; }

        Task<bool> IsPeriodPostedAsync(DateOnly date, CancellationToken cancellationToken = default);

        Task<DateTime> GetMinimumPeriodBasedOnThePostedPeriods(Module module, CancellationToken cancellationToken = default);

        Task<bool> IsPeriodPostedAsync(Module module, DateOnly date, CancellationToken cancellationToken = default);
    }
}
