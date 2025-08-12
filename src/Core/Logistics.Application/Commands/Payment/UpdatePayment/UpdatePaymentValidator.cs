using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
