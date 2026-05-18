using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class DeleteCustomerValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
