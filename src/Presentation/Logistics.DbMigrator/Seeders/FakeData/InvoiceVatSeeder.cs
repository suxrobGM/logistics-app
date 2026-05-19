using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Backfills VAT onto EU demo load invoices after loads and trips have been generated.
/// </summary>
internal sealed class InvoiceVatSeeder(
    ILogger<InvoiceVatSeeder> logger,
    IInvoiceTaxApplier taxApplier) : SeederBase(logger)
{
    public override string Name => nameof(InvoiceVatSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 160;
    public override IReadOnlyList<string> DependsOn => [nameof(LoadSeeder), nameof(TripSeeder)];

    public override Task<bool> ShouldSkipAsync(
        SeederContext context,
        CancellationToken cancellationToken = default) => Task.FromResult(false);

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        var tenant = context.CurrentTenant;
        var region = context.Region;
        if (tenant is null || region?.Region != Region.EU)
        {
            return;
        }

        LogStarting();

        var invoices = await context.TenantUnitOfWork.Repository<LoadInvoice>()
            .GetListAsync(ct: cancellationToken);
        var updated = 0;

        foreach (var invoice in invoices)
        {
            if (invoice.Customer.Address is null || invoice.LineItems.Count == 0)
            {
                continue;
            }

            await taxApplier.ApplyAsync(invoice, cancellationToken);
            updated++;
        }

        if (updated > 0)
        {
            await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        LogCompleted(updated);
    }
}
