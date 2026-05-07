using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Commands;

internal sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Name).NotEmpty();

        RuleFor(i => i.Address!)
            .SetValidator(new AddressValidator())
            .When(i => i.Address is not null);
    }
}
