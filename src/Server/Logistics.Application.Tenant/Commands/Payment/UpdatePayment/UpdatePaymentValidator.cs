using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdatePaymentValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
