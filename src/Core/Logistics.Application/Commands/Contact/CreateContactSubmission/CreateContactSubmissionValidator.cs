using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateContactSubmissionValidator : AbstractValidator<CreateContactSubmissionCommand>
{
    public CreateContactSubmissionValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.Subject)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
