using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodsValidator : AbstractValidator<GetPaymentMethodsQuery>
{
    public GetPaymentMethodsValidator()
    {
    }
}
