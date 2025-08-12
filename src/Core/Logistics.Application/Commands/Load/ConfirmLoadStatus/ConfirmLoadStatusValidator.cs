using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class ConfirmLoadStatusValidator : AbstractValidator<ConfirmLoadStatusCommand>
{
    public ConfirmLoadStatusValidator()
    {
        RuleFor(i => i.DriverId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
        RuleFor(i => i.LoadStatus).NotNull();
    }
}
