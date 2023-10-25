using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateInvoiceHandler : RequestHandler<CreateInvoiceCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateInvoiceHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateInvoiceCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.LoadId);

        if (load is null)
        {
            return ResponseResult.CreateError($"Could not find a load with ID '{req.LoadId}'");
        }
        
        var customer = await _tenantRepository.GetAsync<Customer>(req.CustomerId);

        if (customer is null)
        {
            return ResponseResult.CreateError($"Could not find a customer with ID '{req.CustomerId}'");
        }

        var payment = new Payment
        {
            Method = req.PaymentMethod,
            PaymentFor = PaymentFor.Invoice,
            Amount = req.PaymentAmount
        };
        
        var invoice = new Invoice
        {
            CustomerId = req.CustomerId,
            LoadId = req.LoadId,
            Payment = payment
        };
        
        await _tenantRepository.AddAsync(invoice);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
