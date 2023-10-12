using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
