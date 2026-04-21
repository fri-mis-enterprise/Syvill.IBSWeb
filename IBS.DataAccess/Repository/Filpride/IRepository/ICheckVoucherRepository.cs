using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsPayable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICheckVoucherRepository : IRepository<CheckVoucherHeader>
    {
        Task<string> GenerateCodeAsync(string company, string type, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeMultipleInvoiceAsync(string company, string type, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeMultiplePaymentAsync(string company, string type, CancellationToken cancellationToken = default);

        Task UpdateInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default);

        Task UpdateMultipleInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default);

        Task PostAsync(CheckVoucherHeader header, IEnumerable<CheckVoucherDetail> details, CancellationToken cancellationToken = default);
    }
}
