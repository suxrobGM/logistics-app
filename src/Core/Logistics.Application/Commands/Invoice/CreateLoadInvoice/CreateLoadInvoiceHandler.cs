using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadInvoiceHandler : RequestHandler<CreateLoadInvoiceCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateLoadInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateLoadInvoiceCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find a load with ID '{req.LoadId}'");
        }
        
        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'");
        }

        var payment = new Payment
        {
            Method = req.PaymentMethod,
            Amount = req.PaymentAmount
        };
        
        var invoice = new LoadInvoice
        {
            Total = req.PaymentAmount,
            CustomerId = req.CustomerId,
            LoadId = req.LoadId,
        };
        
        invoice.ApplyPayment(payment);
        await _tenantUow.Repository<Invoice>().AddAsync(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
