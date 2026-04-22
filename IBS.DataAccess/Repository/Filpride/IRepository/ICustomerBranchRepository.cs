using IBS.DataAccess.Repository.IRepository;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICustomerBranchRepository: IRepository<CustomerBranch>
    {
        Task UpdateAsync(CustomerBranch model, CancellationToken cancellationToken);
    }
}
