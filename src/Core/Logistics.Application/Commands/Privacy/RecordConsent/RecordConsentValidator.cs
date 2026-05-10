using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RecordConsentValidator : AbstractValidator<RecordConsentCommand>
{
    public RecordConsentValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ConsentType).IsInEnum();
    }
}
