using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads;

internal sealed class LoadLookupBuilder(ITenantUnitOfWork uow) : ILoadLookupBuilder
{
    public async Task<LoadLookup> BuildAsync(IReadOnlyCollection<Load> loads, CancellationToken ct = default)
    {
        var loadIds = loads.Select(l => l.Id).ToArray();

        var containerIds = loads
            .Select(l => l.ContainerId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        var terminalIds = loads.Select(l => l.OriginTerminalId)
            .Concat(loads.Select(l => l.DestinationTerminalId))
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        var containers = containerIds.Length == 0
            ? []
            : await uow.Repository<Container>().Query()
                .Where(c => containerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => new ContainerRef(
                    c.Number,
                    c.IsoType,
                    c.LastFreeDay,
                    c.CustomsHold || c.FreightHold || c.LineHold || c.OtherHold), ct);

        var terminals = terminalIds.Length == 0
            ? []
            : await uow.Repository<Terminal>().Query()
                .Where(t => terminalIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => new TerminalRef(t.Name, t.Code), ct);

        var truckIds = loads.Select(l => l.AssignedTruckId).OfType<Guid>().Distinct().ToArray();
        var trucks = truckIds.Length == 0
            ? []
            : await uow.Repository<Truck>().Query()
                .Where(t => truckIds.Contains(t.Id))
                .ToListAsync(ct);

        var employeeIds = loads.Select(l => l.AssignedDispatcherId)
            .Concat(trucks.Select(t => t.MainDriverId))
            .Concat(trucks.Select(t => t.SecondaryDriverId))
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        var employeeNames = employeeIds.Length == 0
            ? []
            : (await uow.Repository<Employee>().Query()
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync(ct))
                .ToDictionary(e => e.Id, e => e.GetFullName());

        var trips = loadIds.Length == 0
            ? []
            : (await uow.Repository<TripStop>().Query()
                .Where(s => loadIds.Contains(s.LoadId))
                .Select(s => new { s.LoadId, s.Trip.Id, s.Trip.Name, s.Trip.Number })
                .ToListAsync(ct))
                .GroupBy(s => s.LoadId)
                .ToDictionary(g => g.Key, g => new TripRef(g.First().Id, g.First().Name, g.First().Number));

        // The collections ride along in the same query instead of a lazy SELECT per invoice.
        var invoices = loadIds.Length == 0
            ? []
            : await uow.Repository<LoadInvoice>().Query()
                .Where(i => loadIds.Contains(i.LoadId))
                .Select(i => new InvoiceRows(i, i.Payments.ToList(), i.LineItems.ToList()))
                .ToListAsync(ct);

        var customerIds = loads.Select(l => l.CustomerId)
            .Concat(invoices.Select(i => i.Invoice.CustomerId))
            .Distinct()
            .ToArray();
        var customers = customerIds.Length == 0
            ? []
            : (await uow.Repository<Customer>().Query()
                .Where(c => customerIds.Contains(c.Id))
                .ToListAsync(ct))
                .ToDictionary(c => c.Id, c => c.ToDto());

        var loadNumbers = loads.ToDictionary(l => l.Id, l => l.Number);
        var invoicesByLoad = invoices.ToDictionary(r => r.Invoice.LoadId, r => r.Invoice.ToDto(
            r.Payments,
            r.LineItems,
            loadNumbers[r.Invoice.LoadId],
            customers.GetValueOrDefault(r.Invoice.CustomerId)));

        var truckRefs = trucks.ToDictionary(t => t.Id, t => new TruckRef(
            t.Number,
            [.. new[] { t.MainDriverId, t.SecondaryDriverId }
                .Select(id => id is { } key ? employeeNames.GetValueOrDefault(key) : null)
                .OfType<string>()],
            t.CurrentAddress,
            t.CurrentLocation));

        return new LoadLookup(containers, terminals, customers, employeeNames, truckRefs, trips, invoicesByLoad);
    }

    private sealed record InvoiceRows(LoadInvoice Invoice, List<Payment> Payments, List<InvoiceLineItem> LineItems);
}
