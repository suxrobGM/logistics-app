using System.Globalization;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal sealed class LoadChargeService(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier) : ILoadChargeService
{
    public async Task ReplaceAccessorialsAsync(
        Load load, IReadOnlyList<LoadAccessorialDto> accessorials, CancellationToken ct = default)
    {
        var keep = accessorials.Where(a => a.Id is not null).Select(a => a.Id!.Value).ToHashSet();
        RemoveAccessorials(load, a => !keep.Contains(a.Id));

        foreach (var input in accessorials)
        {
            var existing = input.Id is { } id ? load.Accessorials.FirstOrDefault(a => a.Id == id) : null;
            if (existing is not null)
            {
                existing.Name = input.Name;
                existing.LineItemType = input.LineItemType;
                existing.Unit = input.Unit;
                existing.Rate = input.Rate;
                existing.Quantity = input.Quantity;
                continue;
            }

            await AddAccessorialAsync(load, NewAccessorial(load, input), ct);
        }
    }

    public async Task ReplaceAccessorialsAsync(
        Load load, Func<LoadAccessorial, bool> owns, IReadOnlyList<LoadAccessorial> fresh, CancellationToken ct = default)
    {
        RemoveAccessorials(load, owns);
        foreach (var accessorial in fresh)
        {
            await AddAccessorialAsync(load, accessorial, ct);
        }
    }

    public async Task<bool> SyncInvoiceAsync(Load load, CancellationToken ct = default)
    {
        var invoice = await tenantUow.Repository<LoadInvoice>().GetAsync(i => i.LoadId == load.Id, ct);
        if (invoice is null || invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            return false;
        }

        var (removed, added) = invoice.ReplaceLoadCharges(load);
        var lineRepository = tenantUow.Repository<InvoiceLineItem>();
        foreach (var line in removed)
        {
            lineRepository.Delete(line);
        }

        await lineRepository.AddRangeAsync(added, ct);

        await taxApplier.ApplyAsync(invoice, ct);
        tenantUow.Repository<Invoice>().Update(invoice);
        return true;
    }

    public async Task ApplyDetentionAsync(TripStop stop, CancellationToken ct = default)
    {
        var load = stop.Load;
        var terms = DetentionTerms.For(tenantUow.GetCurrentTenant().Settings, load.Customer);
        var detention = DetentionCalculator.Calculate(stop, DateTime.UtcNow, terms);
        var existing = load.FindDetention(stop.Id);

        if (detention is not { IsOngoing: false, Charge: > 0, RatePerHour: { } rate })
        {
            if (existing is null)
            {
                return;
            }

            RemoveAccessorials(load, a => a == existing);
        }
        else
        {
            var place = stop.Type == TripStopType.PickUp ? "pickup" : "delivery";
            var name = string.Create(CultureInfo.InvariantCulture,
                $"Detention at {place} ({detention.BillableHours:0.##} h past {detention.FreeMinutes / 60m:0.##} h free)");

            if (existing is null)
            {
                existing = new LoadAccessorial
                {
                    LoadId = load.Id,
                    Load = load,
                    TripStopId = stop.Id,
                    Name = name,
                    LineItemType = InvoiceLineItemType.Detention,
                    Unit = AccessorialUnit.PerHour
                };
                await AddAccessorialAsync(load, existing, ct);
            }

            existing.Name = name;
            existing.Rate = rate;
            existing.Quantity = detention.BillableHours;
        }

        await SyncInvoiceAsync(load, ct);
    }

    /// <summary>A removed accessorial must leave the repository too, or EF keeps the row.</summary>
    private void RemoveAccessorials(Load load, Func<LoadAccessorial, bool> predicate)
    {
        var repository = tenantUow.Repository<LoadAccessorial>();
        foreach (var stale in load.Accessorials.Where(predicate).ToList())
        {
            load.Accessorials.Remove(stale);
            repository.Delete(stale);
        }
    }

    private async Task AddAccessorialAsync(Load load, LoadAccessorial accessorial, CancellationToken ct)
    {
        load.Accessorials.Add(accessorial);
        await tenantUow.Repository<LoadAccessorial>().AddAsync(accessorial, ct);
    }

    internal static LoadAccessorial NewAccessorial(Load load, LoadAccessorialDto input) => new()
    {
        LoadId = load.Id,
        Load = load,
        AccessorialChargeId = input.AccessorialChargeId,
        Name = input.Name,
        LineItemType = input.LineItemType,
        Unit = input.Unit,
        Rate = input.Rate,
        Quantity = input.Quantity
    };
}
