using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
    }
}
