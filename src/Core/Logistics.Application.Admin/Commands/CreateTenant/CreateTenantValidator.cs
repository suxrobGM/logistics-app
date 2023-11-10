using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(RegexPatterns.TENANT_NAME);
    }
}
