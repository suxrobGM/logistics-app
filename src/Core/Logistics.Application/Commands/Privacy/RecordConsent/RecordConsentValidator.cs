using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RecordConsentValidator : AbstractValidator<RecordConsentCommand>
{
    public RecordConsentValidator()
    {
        RuleFor(c => c)
            .Must(c => c.UserId.HasValue || c.AnonymousId.HasValue)
            .WithMessage("Either UserId or AnonymousId must be supplied.");
    }
}
