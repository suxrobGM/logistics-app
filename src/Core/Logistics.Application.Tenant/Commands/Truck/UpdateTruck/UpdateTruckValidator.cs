using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateTruckValidator : AbstractValidator<UpdateTruckCommand>
{
    public UpdateTruckValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.DriverIncomePercentage).InclusiveBetween(0, 1);
    }
}
