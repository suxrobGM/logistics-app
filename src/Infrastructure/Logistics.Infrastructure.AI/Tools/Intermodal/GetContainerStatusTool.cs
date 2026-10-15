using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Container = Logistics.Domain.Entities.Container;

namespace Logistics.Infrastructure.AI.Tools.Intermodal;

/// <summary>
/// Looks up an intermodal container by ISO 6346 number (or id), so the agent knows where the box is
/// before assigning the load that carries it.
/// </summary>
internal sealed class GetContainerStatusTool(ITenantUnitOfWork tenantUow)
    : AgentTool<GetContainerStatusTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("ISO 6346 container number, e.g. 'MSCU1234567'")]
        public string? ContainerNumber { get; init; }

        [Description("Container ID (GUID) - use only when the number is unknown")]
        public Guid? ContainerId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_container_status",
        "Look up an intermodal container by its ISO 6346 number. Returns status (Empty, Loaded, AtPort, InTransit, Delivered, Returned), ISO type, laden flag, gross weight, seal, booking reference, bill of lading, the terminal the box is currently at, the load it is linked to, and its drayage state: steamship line, demurrage (last free day) and per-diem (empty return) clocks with days remaining/over and accrued charge, chassis days, customs/freight/line holds and pickup availability, pickup/return appointments, the empty return terminal and any street turn. Call this for any load that reports a container_number before assigning it.")
    {
        RequiredFeature = TenantFeature.IntermodalContainers,
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var number = input.ContainerNumber?.Trim();
        Container? container;

        if (!string.IsNullOrEmpty(number))
        {
            // Dispatchers type ISO 6346 numbers in either case, but stored numbers are canonical -
            // so normalise the term and compare exactly, which the unique index can serve.
            var normalized = Container.NormalizeNumber(number);
            container = await tenantUow.Repository<Container>()
                .GetAsync(c => c.Number == normalized, ct);
        }
        else if (input.ContainerId is { } containerId)
        {
            container = await tenantUow.Repository<Container>().GetByIdAsync(containerId, ct);
        }
        else
        {
            return ToolResult.Error("Provide container_number (ISO 6346) or container_id");
        }

        if (container is null)
            return ToolResult.Error($"No container found for {number ?? input.ContainerId?.ToString()}");

        // Container has no Loads collection - the link is one-way from Load.ContainerId.
        var load = await tenantUow.Repository<Load>()
            .GetAsync(l => l.ContainerId == container.Id, ct);

        var terminal = container.CurrentTerminal;
        var emptyReturn = container.EmptyReturnTerminal;
        var now = DateTime.UtcNow;
        var freeTime = container.GetFreeTime(now);

        return ToolResult.Ok(new
        {
            id = container.Id,
            number = container.Number,
            iso_type = container.IsoType.ToString(),
            iso_type_description = container.IsoType.GetDescription(),
            status = container.Status.ToString(),
            is_laden = container.IsLaden,
            gross_weight = container.GrossWeight,
            seal_number = container.SealNumber,
            booking_reference = container.BookingReference,
            bill_of_lading_number = container.BillOfLadingNumber,
            notes = container.Notes,
            current_terminal = terminal is null
                ? null
                : new
                {
                    id = terminal.Id,
                    name = terminal.Name,
                    code = terminal.Code,
                    type = terminal.Type.ToString(),
                    country_code = terminal.CountryCode,
                    address = GetTerminalInfoTool.FormatAddress(terminal.Address)
                },
            loaded_at = container.LoadedAt,
            delivered_at = container.DeliveredAt,
            returned_at = container.ReturnedAt,
            steamship_line = container.SteamshipLineScac is null && container.SteamshipLineName is null
                ? null
                : new { scac = container.SteamshipLineScac, name = container.SteamshipLineName },
            picked_up_at = container.PickedUpAt,
            demurrage = FormatClock(freeTime.Demurrage),
            per_diem = FormatClock(freeTime.PerDiem),
            chassis = container.ChassisNumber is null && container.ChassisProvider is null
                ? null
                : new
                {
                    number = container.ChassisNumber,
                    provider = container.ChassisProvider?.GetDescription(),
                    days = freeTime.Chassis.Days,
                    charge = freeTime.Chassis.Charge
                },
            total_accrued_charge = freeTime.TotalCharge,
            holds = new
            {
                on_hold = container.IsOnHold,
                customs = container.CustomsHold,
                freight = container.FreightHold,
                line = container.LineHold,
                other = container.OtherHold
            },
            available_for_pickup = container.IsAvailableForPickup(now),
            available_for_pickup_at = container.AvailableForPickupAt,
            pickup_appointment = FormatAppointment(
                container.PickupAppointmentNumber, container.PickupAppointmentFrom, container.PickupAppointmentTo),
            return_appointment = FormatAppointment(
                container.ReturnAppointmentNumber, container.ReturnAppointmentFrom, container.ReturnAppointmentTo),
            empty_return_terminal = emptyReturn is null
                ? null
                : new { id = emptyReturn.Id, name = emptyReturn.Name, code = emptyReturn.Code },
            street_turned_at = container.StreetTurnedAt,
            street_turn_booking_reference = container.StreetTurnBookingReference,
            linked_load = load is null
                ? null
                : new
                {
                    id = load.Id,
                    number = load.Number,
                    name = load.Name,
                    status = load.Status.ToString()
                }
        });
    }

    private static object? FormatClock(FreeTimeClock clock) => clock.Status == FreeTimeStatus.NotSet
        ? null
        : new
        {
            deadline = clock.Deadline,
            status = clock.Status.ToString(),
            days_remaining = clock.DaysRemaining,
            days_over = clock.DaysOver,
            rate_per_day = clock.RatePerDay,
            accrued_charge = clock.Charge
        };

    private static object? FormatAppointment(string? number, DateTime? from, DateTime? to) =>
        number is null && from is null
            ? null
            : new { number, window_from = from, window_to = to };
}
