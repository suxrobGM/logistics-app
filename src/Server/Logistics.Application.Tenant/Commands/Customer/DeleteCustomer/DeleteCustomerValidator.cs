using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteCustomerValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
