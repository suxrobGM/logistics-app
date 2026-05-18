using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

internal sealed class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
