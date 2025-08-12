using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
