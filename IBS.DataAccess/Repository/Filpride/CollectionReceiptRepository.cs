using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.Enums;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using IBS.DTOs;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository.Filpride
{
    public class CollectionReceiptRepository: Repository<CollectionReceipt>, ICollectionReceiptRepository
    {
        private readonly ApplicationDbContext _db;

        public CollectionReceiptRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public async Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                nameof(DocumentType.Documented) => await GenerateCodeForDocumented(cancellationToken),
                nameof(DocumentType.Undocumented) => await GenerateCodeForUnDocumented(cancellationToken),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        private async Task<string> GenerateCodeForDocumented(CancellationToken cancellationToken = default)
        {
            var lastCr = await _db
                .CollectionReceipts
                .AsNoTracking()
                .OrderByDescending(x => x.CollectionReceiptNo!.Length)
                .ThenByDescending(x => x.CollectionReceiptNo)
                .FirstOrDefaultAsync(x =>
                        x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastCr == null)
            {
                return "CR0000000001";
            }

            var lastSeries = lastCr.CollectionReceiptNo!;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(CancellationToken cancellationToken = default)
        {
            var lastCr = await _db
                .CollectionReceipts
                .AsNoTracking()
                .OrderByDescending(x => x.CollectionReceiptNo!.Length)
                .ThenByDescending(x => x.CollectionReceiptNo)
                .FirstOrDefaultAsync(x =>
                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastCr == null)
            {
                return "CRU000000001";
            }

            var lastSeries = lastCr.CollectionReceiptNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public async Task PostAsync(CollectionReceipt collectionReceipt, CancellationToken cancellationToken = default)
        {
            var ledgers = new List<GeneralLedgerBook>();
            var accountTitlesDto = await GetListOfAccountTitleDto(cancellationToken);
            var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                  throw new ArgumentException("Account title '101010100' not found.");
            var arTradeTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020100") ??
                               throw new ArgumentException("Account title '101020100' not found.");
            var arTradeCwt = accountTitlesDto.Find(c => c.AccountNumber == "101020200") ??
                             throw new ArgumentException("Account title '101020200' not found.");
            var arTradeCwv = accountTitlesDto.Find(c => c.AccountNumber == "101020300") ??
                             throw new ArgumentException("Account title '101020300' not found.");
            var cwt = accountTitlesDto.Find(c => c.AccountNumber == "101060400") ??
                      throw new ArgumentException("Account title '101060400' not found.");
            var cwv = accountTitlesDto.Find(c => c.AccountNumber == "101060600") ??
                      throw new ArgumentException("Account title '101060600' not found.");

            collectionReceipt.ReceiptDetails = await _db.CollectionReceiptDetails
                .Where(rd => rd.CollectionReceiptId == collectionReceipt.CollectionReceiptId)
                .ToListAsync(cancellationToken);

            var customerName = collectionReceipt.ServiceInvoice!.Customer!.CustomerName;

            if (collectionReceipt.CashAmount > 0 || collectionReceipt.CheckAmount > 0 ||
                collectionReceipt.ManagersCheckAmount > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = cashInBankTitle.AccountId,
                        AccountNo = cashInBankTitle.AccountNumber,
                        AccountTitle = cashInBankTitle.AccountName,
                        Debit =
                            collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                            collectionReceipt.ManagersCheckAmount,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = SubAccountType.BankAccount,
                        SubAccountId = collectionReceipt.BankId,
                        SubAccountName = collectionReceipt.BankId.HasValue
                            ? $"{collectionReceipt.BankAccountNumber} {collectionReceipt.BankAccountName}"
                            : null,
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.EWT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = cwt.AccountId,
                        AccountNo = cwt.AccountNumber,
                        AccountTitle = cwt.AccountName,
                        Debit = collectionReceipt.EWT,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.WVAT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = cwv.AccountId,
                        AccountNo = cwv.AccountNumber,
                        AccountTitle = cwv.AccountName,
                        Debit = collectionReceipt.WVAT,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.CashAmount > 0 || collectionReceipt.CheckAmount > 0 ||
                collectionReceipt.ManagersCheckAmount > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = arTradeTitle.AccountId,
                        AccountNo = arTradeTitle.AccountNumber,
                        AccountTitle = arTradeTitle.AccountName,
                        Debit = 0,
                        Credit =
                            collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                            collectionReceipt.ManagersCheckAmount,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = SubAccountType.Customer,
                        SubAccountId = collectionReceipt.CustomerId,
                        SubAccountName = customerName,
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.EWT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = arTradeCwt.AccountId,
                        AccountNo = arTradeCwt.AccountNumber,
                        AccountTitle = arTradeCwt.AccountName,
                        Debit = 0,
                        Credit = collectionReceipt.EWT,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.WVAT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = "Collection for Receivable",
                        AccountId = arTradeCwv.AccountId,
                        AccountNo = arTradeCwv.AccountNumber,
                        AccountTitle = arTradeCwv.AccountName,
                        Debit = 0,
                        Credit = collectionReceipt.WVAT,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            await _db.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
        }

        public async Task DepositAsync(CollectionReceipt collectionReceipt,
            CancellationToken cancellationToken = default)
        {
            var ledgers = new List<GeneralLedgerBook>();
            var accountTitlesDto = await GetListOfAccountTitleDto(cancellationToken);
            var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100")
                                  ?? throw new ArgumentException("Account title '101010100' not found.");

            var customerName = collectionReceipt.ServiceInvoice!.Customer!.CustomerName;

            var description =
                $"CR Ref collected from {customerName} for {collectionReceipt.ServiceInvoice!.ServiceInvoiceNo} " +
                $"SV Dated {collectionReceipt.ServiceInvoice.CreatedDate:MMM/dd/yyyy} " +
                $"Check No. {collectionReceipt.CheckNo} issued by " +
                $"{collectionReceipt.BankAccountNumber} {collectionReceipt.BankAccountName}";

            ledgers.Add(
                new GeneralLedgerBook
                {
                    Date = collectionReceipt.TransactionDate,
                    Reference = collectionReceipt.CollectionReceiptNo!,
                    Description = description,
                    AccountId = cashInBankTitle.AccountId,
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountTitle = cashInBankTitle.AccountName,
                    Debit =
                        collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                        collectionReceipt.ManagersCheckAmount,
                    Credit = 0,
                    CreatedBy = collectionReceipt.PostedBy!,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    SubAccountType = SubAccountType.BankAccount,
                    SubAccountId = collectionReceipt.BankId,
                    SubAccountName = collectionReceipt.BankId.HasValue
                        ? $"{collectionReceipt.BankAccountNumber} {collectionReceipt.BankAccountName}"
                        : null,
                    ModuleType = nameof(ModuleType.Collection)
                }
            );

            ledgers.Add(
                new GeneralLedgerBook
                {
                    Date = collectionReceipt.TransactionDate,
                    Reference = collectionReceipt.CollectionReceiptNo!,
                    Description = description,
                    AccountId = cashInBankTitle.AccountId,
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountTitle = cashInBankTitle.AccountName,
                    Debit = 0,
                    Credit =
                        collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                        collectionReceipt.ManagersCheckAmount,
                    CreatedBy = collectionReceipt.PostedBy!,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    ModuleType = nameof(ModuleType.Collection)
                }
            );

            await _db.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveSVPayment(int id, decimal paidAmount, CancellationToken cancellationToken = default)
        {
            var sv = await _db
                .ServiceInvoices
                .FirstOrDefaultAsync(si => si.ServiceInvoiceId == id, cancellationToken);

            if (sv != null)
            {
                var total = paidAmount;
                sv.AmountPaid -= total;
                sv.Balance += total;

                if (sv.IsPaid && sv.PaymentStatus == "Paid" || sv.IsPaid && sv.PaymentStatus == "OverPaid")
                {
                    sv.IsPaid = false;
                    sv.PaymentStatus = "Pending";
                }

                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UndoServiceInvoiceChanges(CollectionReceiptDetail collectionReceiptDetail,
            CancellationToken cancellationToken)
        {
            var sv = await _db
                .ServiceInvoices
                .FirstOrDefaultAsync(si => si.ServiceInvoiceNo == collectionReceiptDetail.InvoiceNo, cancellationToken);

            if (sv == null)
            {
                throw new NullReferenceException("Invoice Not Found.");
            }

            sv.AmountPaid -= collectionReceiptDetail.Amount;
            sv.Balance += collectionReceiptDetail.Amount;
            sv.IsPaid = false;
            sv.PaymentStatus = "Pending";

            if (sv.Balance < 0)
            {
                sv.PaymentStatus = "OverPaid";
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateSV(int id, decimal paidAmount, CancellationToken cancellationToken = default)
        {
            var sv = await _db
                .ServiceInvoices
                .FirstOrDefaultAsync(si => si.ServiceInvoiceId == id, cancellationToken);

            if (sv != null)
            {
                var netDiscount = sv.Total - sv.Discount;

                var total = paidAmount;
                sv.AmountPaid += total;
                sv.Balance = netDiscount - sv.AmountPaid;

                if (sv.Balance == 0 && sv.AmountPaid == netDiscount)
                {
                    sv.IsPaid = true;
                    sv.PaymentStatus = "Paid";
                }
                else if (sv.AmountPaid > netDiscount)
                {
                    sv.IsPaid = true;
                    sv.PaymentStatus = "OverPaid";
                }

                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public override async Task<IEnumerable<CollectionReceipt>> GetAllAsync(
            Expression<Func<CollectionReceipt, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<CollectionReceipt> query = dbSet
                .Include(cr => cr.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Service)
                .Include(cr => cr.BankAccount);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public override async Task<CollectionReceipt?> GetAsync(Expression<Func<CollectionReceipt, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(cr => cr.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Service)
                .Include(cr => cr.BankAccount)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override IQueryable<CollectionReceipt> GetAllQuery(
            Expression<Func<CollectionReceipt, bool>>? filter = null)
        {
            IQueryable<CollectionReceipt> query = dbSet
                .Include(cr => cr.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Customer)
                .Include(cr => cr.ServiceInvoice)
                .ThenInclude(sv => sv!.Service)
                .Include(cr => cr.BankAccount)
                .Include(c => c.ReceiptDetails)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task ReturnedCheck(string crNo, string company, string userName,
            CancellationToken cancellationToken = default)
        {
            var originalEntries = await _db.GeneralLedgerBooks
                .Where(x => x.Reference == crNo)
                .ToListAsync(cancellationToken);

            var reversalEntries = new List<GeneralLedgerBook>();

            foreach (var originalEntry in originalEntries)
            {
                var reversalEntry = new GeneralLedgerBook
                {
                    Reference = originalEntry.Reference,
                    AccountNo = originalEntry.AccountNo,
                    AccountTitle = originalEntry.AccountTitle,
                    Description = "Reversal of entries due to returned checks.",
                    Debit = originalEntry.Credit,
                    Credit = originalEntry.Debit,
                    CreatedBy = userName,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    IsPosted = true,
                    AccountId = originalEntry.AccountId,
                    SubAccountType = originalEntry.SubAccountType,
                    SubAccountId = originalEntry.SubAccountId,
                    SubAccountName = originalEntry.SubAccountName,
                    ModuleType = originalEntry.ModuleType,
                };

                reversalEntries.Add(reversalEntry);
            }

            await _db.GeneralLedgerBooks.AddRangeAsync(reversalEntries, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RedepositAsync(CollectionReceipt collectionReceipt,
            CancellationToken cancellationToken = default)
        {
            var ledgers = new List<GeneralLedgerBook>();
            var accountTitlesDto = await GetListOfAccountTitleDto(cancellationToken);
            var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100") ??
                                  throw new ArgumentException("Account title '101010100' not found.");
            var arTradeTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020100") ??
                               throw new ArgumentException("Account title '101020100' not found.");
            var arTradeCwt = accountTitlesDto.Find(c => c.AccountNumber == "101020200") ??
                             throw new ArgumentException("Account title '101020200' not found.");
            var arTradeCwv = accountTitlesDto.Find(c => c.AccountNumber == "101020300") ??
                             throw new ArgumentException("Account title '101020300' not found.");
            var cwt = accountTitlesDto.Find(c => c.AccountNumber == "101060400") ??
                      throw new ArgumentException("Account title '101060400' not found.");
            var cwv = accountTitlesDto.Find(c => c.AccountNumber == "101060600") ??
                      throw new ArgumentException("Account title '101060600' not found.");

            collectionReceipt.ReceiptDetails = await _db.CollectionReceiptDetails
                .Where(rd => rd.CollectionReceiptId == collectionReceipt.CollectionReceiptId)
                .ToListAsync(cancellationToken);

            var customerName = collectionReceipt.ServiceInvoice!.Customer!.CustomerName;

            var description =
                $"CR Ref collected from {customerName} for {collectionReceipt.ServiceInvoice!.ServiceInvoiceNo} " +
                $"SV Dated {collectionReceipt.ServiceInvoice.CreatedDate:MMM/dd/yyyy} " +
                $"Check No. {collectionReceipt.CheckNo} issued by " +
                $"{collectionReceipt.BankAccountNumber} {collectionReceipt.BankAccountName}";

            if (collectionReceipt.CashAmount > 0 || collectionReceipt.CheckAmount > 0 ||
                collectionReceipt.ManagersCheckAmount > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = cashInBankTitle.AccountId,
                        AccountNo = cashInBankTitle.AccountNumber,
                        AccountTitle = cashInBankTitle.AccountName,
                        Debit =
                            collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                            collectionReceipt.ManagersCheckAmount,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = SubAccountType.BankAccount,
                        SubAccountId = collectionReceipt.BankId,
                        SubAccountName = collectionReceipt.BankId.HasValue
                            ? $"{collectionReceipt.BankAccountNumber} {collectionReceipt.BankAccountName}"
                            : null,
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.EWT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = cwt.AccountId,
                        AccountNo = cwt.AccountNumber,
                        AccountTitle = cwt.AccountName,
                        Debit = collectionReceipt.EWT,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.WVAT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = cwv.AccountId,
                        AccountNo = cwv.AccountNumber,
                        AccountTitle = cwv.AccountName,
                        Debit = collectionReceipt.WVAT,
                        Credit = 0,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.CashAmount > 0 || collectionReceipt.CheckAmount > 0 ||
                collectionReceipt.ManagersCheckAmount > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = arTradeTitle.AccountId,
                        AccountNo = arTradeTitle.AccountNumber,
                        AccountTitle = arTradeTitle.AccountName,
                        Debit = 0,
                        Credit =
                            collectionReceipt.CashAmount + collectionReceipt.CheckAmount +
                            collectionReceipt.ManagersCheckAmount,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = SubAccountType.Customer,
                        SubAccountId = collectionReceipt.CustomerId,
                        SubAccountName = customerName,
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.EWT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = arTradeCwt.AccountId,
                        AccountNo = arTradeCwt.AccountNumber,
                        AccountTitle = arTradeCwt.AccountName,
                        Debit = 0,
                        Credit = collectionReceipt.EWT,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            if (collectionReceipt.WVAT > 0)
            {
                ledgers.Add(
                    new GeneralLedgerBook
                    {
                        Date = collectionReceipt.TransactionDate,
                        Reference = collectionReceipt.CollectionReceiptNo!,
                        Description = description,
                        AccountId = arTradeCwv.AccountId,
                        AccountNo = arTradeCwv.AccountNumber,
                        AccountTitle = arTradeCwv.AccountName,
                        Debit = 0,
                        Credit = collectionReceipt.WVAT,
                        CreatedBy = collectionReceipt.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        ModuleType = nameof(ModuleType.Collection)
                    }
                );
            }

            await _db.GeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
