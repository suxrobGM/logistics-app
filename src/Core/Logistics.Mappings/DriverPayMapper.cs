using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DriverPayMapper
{
    [MapperIgnoreSource(nameof(RecurringDeduction.Employee))]
    public static partial RecurringDeductionDto ToDto(this RecurringDeduction entity);

    [MapperIgnoreSource(nameof(CashAdvance.Employee))]
    [MapperIgnoreTarget(nameof(CashAdvanceDto.EmployeeName))]
    private static partial CashAdvanceDto MapCashAdvance(CashAdvance entity);

    extension(CashAdvance entity)
    {
        public CashAdvanceDto ToDto(string? employeeName) =>
            MapCashAdvance(entity) with { EmployeeName = employeeName };
    }
}
