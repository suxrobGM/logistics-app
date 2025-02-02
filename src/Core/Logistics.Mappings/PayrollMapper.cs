using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PayrollMapper
{
    public static PayrollDto ToDto(this Payroll entity)
    {
        return new PayrollDto
        {
            Id = entity.Id,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Employee = entity.Employee.ToDto(),
            Payment = entity.Payment.ToDto()
        };
    }
}
