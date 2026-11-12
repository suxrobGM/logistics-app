using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class EmployeeMapper
{
    extension(Employee entity)
    {
        public EmployeeDto ToDto()
        {
            return entity.ToDto(entity.Role);
        }

        public EmployeeDto ToDto(TenantRole? role)
        {
            return new EmployeeDto
            {
                Id = entity.Id,
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                FullName = entity.GetFullName(),
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                JoinedDate = entity.JoinedDate,
                Salary = entity.Salary,
                SalaryType = entity.SalaryType,
                Status = entity.Status,
                StripeConnectedAccountId = entity.StripeConnectedAccountId,
                Role = role?.ToDto()
            };
        }
    }

}
