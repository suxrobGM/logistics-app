using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class EmployeeMapper
{
    [MapperIgnoreSource(nameof(Employee.Role))]
    [MapperIgnoreSource(nameof(Employee.RecurringDeductions))]
    [MapperIgnoreSource(nameof(Employee.Documents))]
    [MapperIgnoreSource(nameof(Employee.Licenses))]
    [MapperIgnoreSource(nameof(Employee.DispatchedLoads))]
    [MapperIgnoreSource(nameof(Employee.PayrollInvoices))]
    [MapperIgnoreSource(nameof(Employee.TimeEntries))]
    [MapperIgnoreTarget(nameof(EmployeeDto.Role))]
    [MapperIgnoreTarget(nameof(EmployeeDto.LastKnownLocation))]
    [MapperIgnoreTarget(nameof(EmployeeDto.LastKnownLocationLat))]
    [MapperIgnoreTarget(nameof(EmployeeDto.LastKnownLocationLng))]
    [MapPropertyFromSource(nameof(EmployeeDto.FullName), Use = nameof(GetFullName))]
    private static partial EmployeeDto MapEmployee(Employee entity);

    private static string GetFullName(Employee entity) => entity.GetFullName();

    extension(Employee entity)
    {
        public EmployeeDto ToDto()
        {
            return entity.ToDto(entity.Role);
        }

        public EmployeeDto ToDto(TenantRole? role)
        {
            var dto = MapEmployee(entity);
            dto.Role = role?.ToDto();
            return dto;
        }
    }
}
