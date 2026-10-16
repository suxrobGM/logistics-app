using FluentValidation;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class CaptureProofOfDeliveryValidator : AbstractValidator<CaptureProofOfDeliveryCommand>
{
    public CaptureProofOfDeliveryValidator()
    {
        RuleFor(x => x.UnableToSignReason).MaximumLength(500);
    }
}
