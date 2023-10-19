using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentByIdValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
