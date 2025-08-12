using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodValidator : AbstractValidator<GetPaymentMethodQuery>
{
    public GetPaymentMethodValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
