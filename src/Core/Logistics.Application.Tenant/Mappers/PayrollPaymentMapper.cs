using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class PayrollPaymentMapper
{
    public static PayrollPaymentDto ToDto(this PayrollPayment entity)
    {
        return new PayrollPaymentDto
        {
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Employee = entity.Employee?.ToDto(),
            Payment = entity.Payment.ToDto()
        };
    }
}
