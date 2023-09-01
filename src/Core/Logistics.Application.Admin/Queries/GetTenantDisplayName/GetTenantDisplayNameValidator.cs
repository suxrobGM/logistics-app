using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantDisplayNameValidator : AbstractValidator<GetTenantDisplayNameQuery>
{
    public GetTenantDisplayNameValidator()
    {
        RuleFor(i => i).Must(HaveIdOrName)
            .WithMessage("Both tenant's ID and tenant's name are an empty string, specify at least either ID or tenant's name");
    }
    
    private static bool HaveIdOrName(GetTenantDisplayNameQuery query)
    {
        return !string.IsNullOrEmpty(query.Id) || !string.IsNullOrEmpty(query.Name);
    }
}