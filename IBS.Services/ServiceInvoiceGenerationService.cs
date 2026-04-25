using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;
using IBS.Models.Enums;
using IBS.Utility.Constants;

namespace IBS.Services
{
    public interface IServiceInvoiceGenerationService
    {
        Task<ServiceInvoice> CreateAsync(ServiceInvoiceGenerationRequest request,
            CancellationToken cancellationToken = default);
    }

    public class ServiceInvoiceGenerationRequest
    {
        public required string Type { get; init; }

        public int CustomerId { get; init; }

        public int ServiceId { get; init; }

        public DateOnly Period { get; init; }

        public DateOnly DueDate { get; init; }

        public required string Instructions { get; init; }

        public decimal Total { get; init; }

        public decimal Discount { get; init; }

        public required string CreatedBy { get; init; }

        public int? RecurringServiceInvoiceId { get; init; }
    }

    public class ServiceInvoiceGenerationService: IServiceInvoiceGenerationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceInvoiceGenerationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceInvoice> CreateAsync(ServiceInvoiceGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            var customer = await _unitOfWork.Customer
                .GetAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

            var service = await _unitOfWork.Service
                .GetAsync(c => c.ServiceId == request.ServiceId, cancellationToken);

            if (customer == null || service == null)
            {
                throw new InvalidOperationException("Customer or service could not be found.");
            }

            var normalizedPeriod = new DateOnly(request.Period.Year, request.Period.Month, 1);

            var model = new ServiceInvoice
            {
                ServiceInvoiceNo =
                    await _unitOfWork.ServiceInvoice.GenerateCodeAsync(request.Type, cancellationToken),
                ServiceId = service.ServiceId,
                ServiceName = service.Name,
                ServicePercent = service.Percent,
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                CustomerAddress = customer.CustomerAddress,
                CustomerBusinessStyle = customer.BusinessStyle,
                CustomerTin = customer.CustomerTin,
                VatType = request.Type == nameof(DocumentType.Documented)
                    ? customer.VatType
                    : SD.VatType_Exempt,
                HasEwt = customer.WithHoldingTax && request.Type == nameof(DocumentType.Documented),
                HasWvat = customer.WithHoldingVat && request.Type == nameof(DocumentType.Documented),
                CreatedBy = request.CreatedBy,
                Total = request.Total,
                Balance = request.Total,
                Period = normalizedPeriod,
                Instructions = request.Instructions,
                DueDate = request.DueDate,
                Discount = request.Discount,
                Type = request.Type,
                RecurringServiceInvoiceId = request.RecurringServiceInvoiceId
            };

            await _unitOfWork.ServiceInvoice.AddAsync(model, cancellationToken);

            return model;
        }
    }
}
