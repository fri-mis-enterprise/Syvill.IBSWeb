using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;
using IBS.Models.AccountsReceivable;
using IBS.Models.Books;
using IBS.Models.ViewModels;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<bool> HasAlreadyBeginningInventory(int productId, int poId, string company, CancellationToken cancellationToken = default);

        Task AddBeginningInventory(BeginningInventoryViewModel viewModel, string company, CancellationToken cancellationToken = default);

        Task AddPurchaseToInventoryAsync(ReceivingReport receivingReport, CancellationToken cancellationToken = default);

        Task AddSalesToInventoryAsync(DeliveryReceipt deliveryReceipt, CancellationToken cancellationToken = default);

        Task AddActualInventory(ActualInventoryViewModel viewModel, string company, CancellationToken cancellationToken = default);

        Task VoidInventory(Inventory model, CancellationToken cancellationToken = default);

        Task ReCalculateInventoryAsync(List<Inventory> inventories, CancellationToken cancellationToken = default);
    }
}
