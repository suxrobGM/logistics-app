using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(i => i.Name).NotEmpty();

        RuleFor(i => i.Address!)
            .SetValidator(new AddressValidator())
            .When(i => i.Address is not null);
    }
}
