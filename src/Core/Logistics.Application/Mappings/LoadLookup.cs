using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

/// <summary>Container display values for one load.</summary>
public readonly record struct ContainerRef(string Number, ContainerIsoType IsoType, DateOnly? LastFreeDay, bool IsOnHold);

/// <summary>Terminal display values for one load.</summary>
public readonly record struct TerminalRef(string Name, string Code);

/// <summary>Assigned-truck display values for one load.</summary>
public sealed record TruckRef(string Number, string[] DriversNames, Address? CurrentAddress, GeoPoint? CurrentLocation);

/// <summary>The trip a load rides on.</summary>
public readonly record struct TripRef(Guid Id, string Name, long Number);

/// <summary>
/// Pre-resolved values for a batch of loads. Every one of them is a lazy navigation on
/// <c>Load</c>, so reading them inside the mapper costs a SELECT per row - the N+1 that
/// <c>.claude/rules/backend/mapperly.md</c> forbids. Handlers resolve them once and pass this in.
/// </summary>
public sealed class LoadLookup(
    IReadOnlyDictionary<Guid, ContainerRef> containers,
    IReadOnlyDictionary<Guid, TerminalRef> terminals,
    IReadOnlyDictionary<Guid, CustomerDto> customers,
    IReadOnlyDictionary<Guid, string> employeeNames,
    IReadOnlyDictionary<Guid, TruckRef> trucks,
    IReadOnlyDictionary<Guid, TripRef> tripsByLoad,
    IReadOnlyDictionary<Guid, InvoiceDto> invoicesByLoad)
{
    public ContainerRef? FindContainer(Guid? id) =>
        id is { } key && containers.TryGetValue(key, out var found) ? found : null;

    public TerminalRef? FindTerminal(Guid? id) =>
        id is { } key && terminals.TryGetValue(key, out var found) ? found : null;

    public CustomerDto? FindCustomer(Guid id) => customers.GetValueOrDefault(id);

    public string? FindEmployeeName(Guid? id) =>
        id is { } key ? employeeNames.GetValueOrDefault(key) : null;

    public TruckRef? FindTruck(Guid? id) =>
        id is { } key ? trucks.GetValueOrDefault(key) : null;

    public TripRef? FindTrip(Guid loadId) =>
        tripsByLoad.TryGetValue(loadId, out var found) ? found : null;

    public InvoiceDto? FindInvoice(Guid loadId) => invoicesByLoad.GetValueOrDefault(loadId);
}
