using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DriverPayMapper
{
    [MapperIgnoreSource(nameof(RecurringDeduction.DomainEvents))]
    [MapperIgnoreSource(nameof(RecurringDeduction.Employee))]
    [MapperIgnoreSource(nameof(RecurringDeduction.EmployeeId))]
    public static partial RecurringDeductionDto ToDto(this RecurringDeduction entity);

    [MapperIgnoreSource(nameof(CashAdvance.DomainEvents))]
    [MapperIgnoreSource(nameof(CashAdvance.Employee))]
    [MapperIgnoreSource(nameof(CashAdvance.ReviewedById))]
    [MapperIgnoreSource(nameof(CashAdvance.CreatedBy))]
    [MapperIgnoreSource(nameof(CashAdvance.UpdatedAt))]
    [MapperIgnoreSource(nameof(CashAdvance.UpdatedBy))]
    [MapperIgnoreTarget(nameof(CashAdvanceDto.EmployeeName))]
    private static partial CashAdvanceDto MapCashAdvance(CashAdvance entity);

    public static CashAdvanceDto ToDto(this CashAdvance entity, string? employeeName) =>
        MapCashAdvance(entity) with { EmployeeName = employeeName };
}
