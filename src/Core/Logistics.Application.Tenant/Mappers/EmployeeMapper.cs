using Logistics.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class EmployeeMapper
{
    public static Employee ToEntity(this EmployeeDto dto)
    {
        return new Employee
        {
            Id = dto.Id,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            JoinedDate = dto.JoinedDate,
            PhoneNumber = dto.PhoneNumber,
        };
    }

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
            Roles = entity.Roles.Select(i => new TenantRoleDto
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            }).ToArray()
        };
    }
}
