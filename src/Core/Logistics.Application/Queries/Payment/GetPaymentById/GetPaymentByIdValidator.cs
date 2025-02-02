using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentByIdValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
