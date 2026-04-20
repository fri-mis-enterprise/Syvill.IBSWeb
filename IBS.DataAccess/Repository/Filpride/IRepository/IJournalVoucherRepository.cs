using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IJournalVoucherRepository : IRepository<FilprideJournalVoucherHeader>
    {
        Task<string> GenerateCodeAsync(string company, string? type, CancellationToken cancellationToken = default);

        Task PostAsync(FilprideJournalVoucherHeader header,
            IEnumerable<JournalVoucherDetail> details,
            CancellationToken cancellationToken = default);
    }
}
