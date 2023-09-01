using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateTenantValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantValidator()
    {
        const string tenantNamePattern = @"^[a-z]+\d*";
        
        RuleFor(i => i.Id)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .NotEmpty()
            .MinimumLength(4)
            .Matches(tenantNamePattern);
    }
}