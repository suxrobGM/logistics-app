using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

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
            Salary = entity.Salary,
            SalaryType = entity.SalaryType,
            Status = entity.Status,
            Role = entity.Role?.ToDto()
        };
    }
}
