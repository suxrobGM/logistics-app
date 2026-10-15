using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>The signed-in driver asks for an advance; a dispatcher approves or rejects it.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class RequestCashAdvanceCommand : ICommand<Result<CashAdvanceDto>>
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}
