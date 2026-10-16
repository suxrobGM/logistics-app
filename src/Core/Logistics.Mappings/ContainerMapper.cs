using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class ContainerMapper
{
    [MapperIgnoreSource(nameof(Container.CurrentTerminal))]
    [MapperIgnoreSource(nameof(Container.EmptyReturnTerminal))]
    [MapperIgnoreSource(nameof(Container.DomainEvents))]
    [MapperIgnoreSource(nameof(Container.CreatedBy))]
    [MapperIgnoreSource(nameof(Container.UpdatedAt))]
    [MapperIgnoreSource(nameof(Container.UpdatedBy))]
    [MapperIgnoreTarget(nameof(ContainerDto.CurrentTerminalName))]
    [MapperIgnoreTarget(nameof(ContainerDto.CurrentTerminalCode))]
    [MapperIgnoreTarget(nameof(ContainerDto.EmptyReturnTerminalName))]
    [MapperIgnoreTarget(nameof(ContainerDto.EmptyReturnTerminalCode))]
    [MapperIgnoreTarget(nameof(ContainerDto.IsAvailableForPickup))]
    [MapperIgnoreTarget(nameof(ContainerDto.Demurrage))]
    [MapperIgnoreTarget(nameof(ContainerDto.PerDiem))]
    [MapperIgnoreTarget(nameof(ContainerDto.Chassis))]
    [MapperIgnoreTarget(nameof(ContainerDto.TotalCharge))]
    private static partial ContainerDto ToDtoCore(this Container entity);

    [MapperIgnoreSource(nameof(FreeTimeClock.NeedsAttention))]
    private static partial FreeTimeClockDto ToDto(this FreeTimeClock clock);

    private static partial ChassisUsageDto ToDto(this ChassisUsage usage);

    /// <summary>
    /// Single-row mapping: the two terminals come off the navigation properties, which is one lazy
    /// SELECT each. List handlers use the overload taking pre-resolved terminals instead.
    /// </summary>
    public static ContainerDto ToDto(this Container entity, DateTime now)
    {
        var current = entity.CurrentTerminal;
        var emptyReturn = entity.EmptyReturnTerminal;
        return entity.ToDto(
            now,
            current is null ? null : new TerminalRef(current.Name, current.Code),
            emptyReturn is null ? null : new TerminalRef(emptyReturn.Name, emptyReturn.Code));
    }

    public static ContainerDto ToDto(
        this Container entity,
        DateTime now,
        TerminalRef? currentTerminal,
        TerminalRef? emptyReturnTerminal)
    {
        var dto = entity.ToDtoCore();
        var freeTime = entity.GetFreeTime(now);

        dto.CurrentTerminalName = currentTerminal?.Name;
        dto.CurrentTerminalCode = currentTerminal?.Code;
        dto.EmptyReturnTerminalName = emptyReturnTerminal?.Name;
        dto.EmptyReturnTerminalCode = emptyReturnTerminal?.Code;
        dto.IsAvailableForPickup = entity.IsAvailableForPickup(now);
        dto.Demurrage = freeTime.Demurrage.ToDto();
        dto.PerDiem = freeTime.PerDiem.ToDto();
        dto.Chassis = freeTime.Chassis.ToDto();
        dto.TotalCharge = freeTime.TotalCharge;
        return dto;
    }
}
