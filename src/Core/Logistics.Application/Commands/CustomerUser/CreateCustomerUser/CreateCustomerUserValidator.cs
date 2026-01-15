using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateCustomerUserValidator : AbstractValidator<CreateCustomerUserCommand>
{
    public CreateCustomerUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(256)
            .When(x => x.DisplayName is not null)
            .WithMessage("Display name must be 256 characters or less.");
    }
}
