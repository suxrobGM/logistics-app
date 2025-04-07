using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentMethodValidator : AbstractValidator<DeletePaymentMethodCommand>
{
    public DeletePaymentMethodValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.TenantId).NotEmpty();
    }
}
