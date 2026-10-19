using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class RecordFactoringFundingHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<RecordFactoringFundingCommand, Result>
{
    public async Task<Result> Handle(RecordFactoringFundingCommand req, CancellationToken ct)
    {
        var batch = await tenantUow.Repository<FactoringBatch>().GetByIdAsync(req.BatchId, ct);
        if (batch is null)
        {
            return Result.Fail("Schedule of accounts not found.");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var fundedAt = DateTime.SpecifyKind(req.FundedAt, DateTimeKind.Utc);
        var payments = new List<Payment>();
        try
        {
            foreach (var line in req.Invoices)
            {
                var item = batch.Items.FirstOrDefault(i => i.Id == line.ItemId);
                if (item is null)
                {
                    return Result.Fail("One of the invoices is not on this schedule. Reload and try again.");
                }

                if (line.Rejected)
                {
                    item.Reject(line.RejectionReason);
                }
                else
                {
                    payments.Add(item.Fund(line.AdvanceAmount, line.FeeAmount, fundedAt, tenant.CompanyAddress));
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.Repository<Payment>().AddRangeAsync(payments, ct);
        batch.RefreshStatus();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
