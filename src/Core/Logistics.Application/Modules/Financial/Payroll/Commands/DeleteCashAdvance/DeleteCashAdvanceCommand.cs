using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>Removes an advance entered by mistake. One already deducted from a settlement stays.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class DeleteCashAdvanceCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
