using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class ReportRepository: Repository<GeneralLedgerBook>, IReportRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public async Task<List<GeneralLedgerBook>> GetGeneralLedgerBooks(DateOnly dateFrom, DateOnly dateTo,
            CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var generalLedgerBooks = await _db
                .GeneralLedgerBooks
                .Where(i => i.Date >= dateFrom && i.Date <= dateTo && i.IsPosted)
                .Include(i => i.Account)
                .OrderBy(i => i.Date)
                .ThenBy(i => i.Reference)
                .ThenByDescending(i => i.Debit)
                .ToListAsync(cancellationToken);

            return generalLedgerBooks;
        }

        public async Task<List<ServiceInvoice>> GetServiceInvoiceReport(DateOnly dateFrom, DateOnly dateTo,
            string statusFilter = "ValidOnly", CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var query = _db.ServiceInvoices
                .Where(dr =>
                    dr.Period >= dateFrom &&
                    dr.Period <= dateTo);

            // Apply status filter
            if (statusFilter == "ValidOnly")
            {
                query = query.Where(dr => dr.Status == nameof(Status.Posted));
            }
            else if (statusFilter == "InvalidOnly")
            {
                query = query.Where(dr => dr.Status == nameof(Status.Voided));
            }
            // "All" returns Posted, Voided, and Canceled records

            var serviceInvoices = await query
                .Include(dr => dr.Customer)
                .Include(dr => dr.Service)
                .OrderBy(p => p.ServiceInvoiceNo)
                .ToListAsync(cancellationToken);

            return serviceInvoices;
        }

        public async Task<List<CheckVoucherHeader>> GetClearedDisbursementReport(DateOnly dateFrom, DateOnly dateTo,
            CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var checkVoucherHeader = await _db.CheckVoucherHeaders
                .AsNoTracking()
                .Where(cd =>
                    cd.DcrDate >= dateFrom &&
                    cd.DcrDate <= dateTo &&
                    cd.Status == nameof(Status.Posted) &&
                    cd.CvType != nameof(CVType.Invoicing))
                .Include(cd => cd.BankAccount)
                .Include(cd => cd.Details)
                .OrderBy(cd => cd.Date)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            return checkVoucherHeader;
        }

        public async Task<List<CollectionReceipt>> GetCollectionReceiptReport(DateOnly dateFrom, DateOnly dateTo,
            string statusFilter = "ValidOnly", CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var query = _db.CollectionReceipts
                .Where(cr =>
                    cr.TransactionDate >= dateFrom &&
                    cr.TransactionDate <= dateTo);

            // Apply status filter
            if (statusFilter == "ValidOnly")
            {
                query = query.Where(cr => cr.PostedBy != null);
            }
            else if (statusFilter == "InvalidOnly")
            {
                query = query.Where(cr => cr.VoidedBy != null);
            }
            // "All" returns all records

            var collectionReceipts = await query
                .Include(cr => cr.Customer)
                .Include(cr => cr.ServiceInvoice)
                .Include(cr => cr.BankAccount)
                .OrderBy(cr => cr.Customer!.CustomerCode)
                .ThenBy(cr => cr.Customer!.CustomerName)
                .ToListAsync(cancellationToken);

            return collectionReceipts;
        }

        public async Task<List<ServiceInvoice>> GetARPerCustomerReport(DateOnly dateFrom, DateOnly dateTo,
            List<int>? customerIds = null, string statusFilter = "ValidOnly",
            CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var serviceInvoiceQuery = _db.ServiceInvoices
                .Where(x => (customerIds == null || customerIds.Contains(x.CustomerId))
                            && x.Period >= dateFrom && x.Period <= dateTo);

            // Apply status filter
            if (statusFilter == "ValidOnly")
            {
                serviceInvoiceQuery = serviceInvoiceQuery.Where(x => x.Status == nameof(Status.Posted));
            }
            else if (statusFilter == "InvalidOnly")
            {
                serviceInvoiceQuery = serviceInvoiceQuery.Where(x => x.Status == nameof(Status.Voided));
            }
            // "All" returns Posted, Voided, and Canceled records

            // Include necessary related entities
            var serviceInvoices = await serviceInvoiceQuery
                .Include(si => si.Customer)
                .ToListAsync(cancellationToken);

            return serviceInvoices.OrderBy(rr => rr.Period).ToList();
        }


        public async Task<List<JournalVoucherDetail>> GetJournalVoucherReport(DateOnly dateFrom,
            DateOnly dateTo, string statusFilter = "ValidOnly", CancellationToken cancellationToken = default)
        {
            if (dateFrom > dateTo)
            {
                throw new ArgumentException("Date From must not be greater than Date To!");
            }

            var query = _db.JournalVoucherDetails
                .Include(jvd => jvd.JournalVoucherHeader)
                .ThenInclude(jvh => jvh!.CheckVoucherHeader)
                .Where(x => x.JournalVoucherHeader!.Date >= dateFrom
                            && x.JournalVoucherHeader.Date <= dateTo);

            // Apply status filter
            if (statusFilter == "ValidOnly")
            {
                query = query.Where(x => x.JournalVoucherHeader!.PostedBy != null);
            }
            else if (statusFilter == "InvalidOnly")
            {
                query = query.Where(x => x.JournalVoucherHeader!.VoidedBy != null);
            }
            // "All" returns all records without filtering

            var journalVoucherDetails = await query
                .OrderBy(jvd => jvd.JournalVoucherHeader!.Date)
                .ThenBy(jvd => jvd.JournalVoucherHeader!.JournalVoucherHeaderNo)
                .ToListAsync(cancellationToken);


            return journalVoucherDetails;
        }
    }
}
