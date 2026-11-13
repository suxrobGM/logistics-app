using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Operations.TimeEntries;

[Mapper]
public static partial class TimeEntryMapper
{
    [MapperIgnoreSource(nameof(TimeEntry.Employee))]
    [MapperIgnoreSource(nameof(TimeEntry.PayrollInvoice))]
    [MapperIgnoreTarget(nameof(TimeEntryDto.EmployeeName))]
    [MapProperty(nameof(TimeEntry.Type), nameof(TimeEntryDto.Type), Use = nameof(ToLowerName))]
    private static partial TimeEntryDto MapTimeEntry(TimeEntry entity);

    private static string ToLowerName(TimeEntryType type) => type.ToString().ToLowerInvariant();

    extension(TimeEntry entity)
    {
        /// <summary>Single rows only; lists batch the names and use the overload.</summary>
        public TimeEntryDto ToDto() => entity.ToDto(entity.Employee.GetFullName());

        public TimeEntryDto ToDto(string? employeeName)
        {
            var dto = MapTimeEntry(entity);
            dto.EmployeeName = employeeName;
            return dto;
        }
    }
}
