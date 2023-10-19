using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class EmployeeMapper
{
    public static EmployeeDto ToDto(this Employee entity)
    {
        return new EmployeeDto
        {
            Id = entity.Id,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            FullName = entity.GetFullName(),
            PhoneNumber = entity.PhoneNumber,
            JoinedDate = entity.JoinedDate,
            TruckNumber = entity.Truck?.TruckNumber,
            TruckId = entity.TruckId,
            Salary = entity.Salary,
            SalaryType = entity.SalaryType,
            Roles = entity.Roles.Select(i => new TenantRoleDto
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
        };
    }
}
