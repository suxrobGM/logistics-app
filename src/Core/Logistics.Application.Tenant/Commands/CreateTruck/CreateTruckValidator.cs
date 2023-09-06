using FluentValidation;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateTruckValidator : AbstractValidator<CreateTruckCommand>
{
    public CreateTruckValidator()
    {
        RuleFor(i => i.TruckNumber).NotEmpty();
        RuleFor(i => i.DriverId).NotEmpty();
    }
}
