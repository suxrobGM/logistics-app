using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class AddDrayageChargesHandler(
    ITenantUnitOfWork tenantUow,
    ILoadChargeService loadCharges)
    : IRequestHandler<AddDrayageChargesCommand, Result>
{
    /// <summary>The accessorial types this command owns. Storage is billed by hand, so it is not here.</summary>
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

        var fresh = BuildAccessorials(load, container, container.GetFreeTime(DateTime.UtcNow));
        await loadCharges.ReplaceAccessorialsAsync(load, a => DrayageTypes.Contains(a.LineItemType), fresh, ct);

        if (!await loadCharges.SyncInvoiceAsync(load, ct))
        {
            return Result.Fail("This load has no open invoice.");
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>
    /// One per-day accessorial per non-zero charge, so the invoice shows the daily rate times the
    /// chargeable days the customer will check.
    /// </summary>
    internal static List<LoadAccessorial> BuildAccessorials(Load load, Container container, ContainerFreeTime freeTime)
    {
        var accessorials = new List<LoadAccessorial>();

        void Add(InvoiceLineItemType type, string name, decimal? rate, int days, decimal charge)
        {
            if (charge <= 0 || rate is null)
            {
                return;
            }

            accessorials.Add(new LoadAccessorial
            {
                LoadId = load.Id,
                Load = load,
                Name = name,
                LineItemType = type,
                Unit = AccessorialUnit.PerDay,
                Rate = rate.Value,
                Quantity = days
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

        return accessorials;
    }
}
