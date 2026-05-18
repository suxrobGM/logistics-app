using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

internal sealed class RecordConsentValidator : AbstractValidator<RecordConsentCommand>
{
    public RecordConsentValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ConsentType).IsInEnum();
    }
}
