using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetTenantValidator : AbstractValidator<GetTenantQuery>
{
    public GetTenantValidator()
    {
        RuleFor(i => i).Must(HaveIdOrName)
            .WithMessage("Both tenant's ID and tenant's name are an empty string, specify at least either ID or tenant's name");
    }
    
    private static bool HaveIdOrName(GetTenantQuery query)
    {
        return !string.IsNullOrEmpty(query.Id) || !string.IsNullOrEmpty(query.Name);
    }
}