using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class AddDrayageChargesHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IRequestHandler<AddDrayageChargesCommand, Result>
{
    /// <summary>The line types this command owns. Storage is billed by hand, so it is not here.</summary>
    private static readonly InvoiceLineItemType[] DrayageTypes =
        [InvoiceLineItemType.Demurrage, InvoiceLineItemType.PerDiem, InvoiceLineItemType.Chassis];

    public async Task<Result> Handle(AddDrayageChargesCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        if (load.ContainerId is not { } containerId)
        {
            return Result.Fail("This load has no container.");
        }

        var container = await tenantUow.Repository<Container>().GetByIdAsync(containerId, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{containerId}'");
        }

        var invoice = await tenantUow.Repository<LoadInvoice>().GetAsync(i => i.LoadId == load.Id, ct);
        if (invoice is null)
        {
            return Result.Fail("This load has no invoice.");
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            return Result.Fail($"Cannot modify a {invoice.Status.ToString().ToLowerInvariant()} invoice.");
        }

        foreach (var stale in invoice.LineItems.Where(li => DrayageTypes.Contains(li.Type)).ToList())
        {
            tenantUow.Repository<InvoiceLineItem>().Delete(stale);
            invoice.LineItems.Remove(stale);
        }

        var nextOrder = invoice.LineItems.Count == 0 ? 0 : invoice.LineItems.Max(li => li.Order) + 1;
        var lines = BuildLines(container, container.GetFreeTime(DateTime.UtcNow), invoice, nextOrder);

        foreach (var line in lines)
        {
            await tenantUow.Repository<InvoiceLineItem>().AddAsync(line, ct);
            invoice.LineItems.Add(line);
        }

        await taxApplier.ApplyAsync(invoice, ct);
        tenantUow.Repository<Invoice>().Update(invoice);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>
    /// One line per non-zero charge, priced as the daily rate times the chargeable days, so the
    /// invoice shows the arithmetic the customer will check.
    /// </summary>
    internal static List<InvoiceLineItem> BuildLines(
        Container container,
        ContainerFreeTime freeTime,
        Invoice invoice,
        int firstOrder)
    {
        var lines = new List<InvoiceLineItem>();
        var order = firstOrder;

        void Add(InvoiceLineItemType type, string description, decimal? rate, int days, decimal charge)
        {
            if (charge <= 0 || rate is null)
            {
                return;
            }

            lines.Add(new InvoiceLineItem
            {
                InvoiceId = invoice.Id,
                Description = description,
                Type = type,
                Amount = invoice.Total with { Amount = rate.Value },
                Quantity = days,
                Order = order++
            });
        }

        var demurrage = freeTime.Demurrage;
        Add(InvoiceLineItemType.Demurrage,
            $"Demurrage - {container.Number}, {demurrage.DaysOver} day(s) after last free day {demurrage.Deadline:yyyy-MM-dd}",
            demurrage.RatePerDay, demurrage.DaysOver, demurrage.Charge);

        var perDiem = freeTime.PerDiem;
        Add(InvoiceLineItemType.PerDiem,
            $"Per diem - {container.Number}, {perDiem.DaysOver} day(s) after empty return deadline {perDiem.Deadline:yyyy-MM-dd}",
            perDiem.RatePerDay, perDiem.DaysOver, perDiem.Charge);

        var chassis = freeTime.Chassis;
        var chassisLabel = container.ChassisNumber is null ? "" : $" {container.ChassisNumber}";
        Add(InvoiceLineItemType.Chassis,
            $"Chassis{chassisLabel} - {container.Number}, {chassis.Days} day(s)",
            chassis.RatePerDay, chassis.Days, chassis.Charge);

        return lines;
    }
}
