using IBS.DataAccess.Repository.IRepository;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task<string> GetLastNumber(CancellationToken cancellationToken = default);

        Task<bool> IsServicesExist(string serviceName, CancellationToken cancellationToken = default);
    }
}
