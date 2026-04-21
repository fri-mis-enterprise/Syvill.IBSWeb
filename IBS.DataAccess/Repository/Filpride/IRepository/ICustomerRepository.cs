using IBS.DataAccess.Repository.IRepository;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<bool> IsTinNoExistAsync(string tin, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(Customer model, CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetCustomerBranchesSelectListAsync(int customerId, CancellationToken cancellationToken = default);
    }
}
