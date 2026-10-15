using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>Approves or rejects a driver's requested advance.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class ReviewCashAdvanceCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public bool Approve { get; set; }
}
