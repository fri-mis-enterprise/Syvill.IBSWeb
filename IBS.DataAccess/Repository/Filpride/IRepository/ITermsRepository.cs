using System.Linq.Expressions;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ITermsRepository : IRepository<Terms>
    {
        Task UpdateAsync(Terms model, CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetTermsListAsyncByCode(CancellationToken cancellationToken = default);
    }
}
