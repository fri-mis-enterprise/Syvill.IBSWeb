using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IServiceInvoiceRepository: IRepository<ServiceInvoice>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task PostAsync(ServiceInvoice model, CancellationToken cancellationToken = default);
    }
}
