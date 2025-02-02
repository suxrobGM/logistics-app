using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Name).NotEmpty();
    }
}
