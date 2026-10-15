using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class ChargeBackFactoredInvoiceHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<ChargeBackFactoredInvoiceCommand, Result>
{
    public async Task<Result> Handle(ChargeBackFactoredInvoiceCommand req, CancellationToken ct)
    {
        var batch = await tenantUow.Repository<FactoringBatch>().GetByIdAsync(req.BatchId, ct);
        var item = batch?.Items.FirstOrDefault(i => i.Id == req.ItemId);
        if (batch is null || item is null)
        {
            return Result.Fail("Invoice not found on this schedule.");
        }

        Payment? salePayment;
        try
        {
            salePayment = item.ChargeBack(DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        if (salePayment is not null)
        {
            tenantUow.Repository<Payment>().Delete(salePayment);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
