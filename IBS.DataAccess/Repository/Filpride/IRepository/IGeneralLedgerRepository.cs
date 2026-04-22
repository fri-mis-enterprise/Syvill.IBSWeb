using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Books;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IGeneralLedgerRepository: IRepository<GeneralLedgerBook>
    {
        Task ReverseEntries(string? reference, CancellationToken cancellationToken = default);
    }
}
