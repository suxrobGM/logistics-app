using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        const string namePattern = @"^[a-z]+\d*";

        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(namePattern);
    }
}