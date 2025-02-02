using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(RegexPatterns.TenantName);
    }
}
