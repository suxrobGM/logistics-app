using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeletePayrollValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePayrollValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
