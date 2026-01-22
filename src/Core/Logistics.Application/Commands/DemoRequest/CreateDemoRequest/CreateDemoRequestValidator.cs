using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateDemoRequestValidator : AbstractValidator<CreateDemoRequestCommand>
{
    public CreateDemoRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.FleetSize)
            .MaximumLength(50);

        RuleFor(x => x.Message)
            .MaximumLength(2000);
    }
}
