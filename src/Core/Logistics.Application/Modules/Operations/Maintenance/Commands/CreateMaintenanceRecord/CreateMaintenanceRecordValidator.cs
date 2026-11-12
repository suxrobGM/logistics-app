using FluentValidation;

namespace Logistics.Application.Modules.Operations.Maintenance.Commands;

internal sealed class CreateMaintenanceRecordValidator : AbstractValidator<CreateMaintenanceRecordCommand>
{
    public CreateMaintenanceRecordValidator()
    {
        RuleFor(i => i.Description).NotEmpty();
        RuleFor(i => i.LaborCost).GreaterThanOrEqualTo(0);
        RuleFor(i => i.PartsCost).GreaterThanOrEqualTo(0);

        RuleForEach(i => i.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.PartName).NotEmpty();
            part.RuleFor(p => p.Quantity).GreaterThan(0);
            part.RuleFor(p => p.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
