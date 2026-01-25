using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class NewLoadCreatedHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<NewLoadCreatedHandler> logger)
    : IDomainEventHandler<NewLoadCreatedEvent>
{
    public async Task Handle(NewLoadCreatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Created a new load {LoadId}", @event.LoadId);

        // Fetch the load to create the invoice
        var load = await tenantUow.Repository<Load>().GetByIdAsync(@event.LoadId, cancellationToken);
        if (load is null)
        {
            logger.LogWarning("Load {LoadId} not found, cannot create invoice", @event.LoadId);
            return;
        }

        // Check if an invoice already exists for this load
        var existingInvoice = await tenantUow.Repository<LoadInvoice>()
            .GetAsync(i => i.LoadId == load.Id, cancellationToken);

        if (existingInvoice is not null)
        {
            logger.LogInformation("Invoice already exists for load {LoadId}, skipping creation", @event.LoadId);
            return;
        }

        // Create the invoice
        var invoice = new LoadInvoice
        {
            LoadId = load.Id,
            CustomerId = load.CustomerId,
            Total = load.DeliveryCost,
            Status = InvoiceStatus.Draft,
            DueDate = DateTime.UtcNow.AddDays(30) // Default 30-day payment terms
        };

        // Create line items from load details
        // Currently the Load only has DeliveryCost, so we create a single "Freight Charges" line item
        // In the future, if Load has FuelSurcharge, Detention, etc., those can be parsed into separate line items
        var freightLineItem = new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = $"Freight charges for Load #{load.Number}",
            Type = InvoiceLineItemType.BaseRate,
            Amount = load.DeliveryCost,
            Quantity = 1,
            Order = 0
        };

        invoice.LineItems.Add(freightLineItem);

        await tenantUow.Repository<LoadInvoice>().AddAsync(invoice, cancellationToken);
        await tenantUow.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created invoice {InvoiceId} for load {LoadId} with total {Total}",
            invoice.Id, load.Id, invoice.Total.Amount);
    }
}
