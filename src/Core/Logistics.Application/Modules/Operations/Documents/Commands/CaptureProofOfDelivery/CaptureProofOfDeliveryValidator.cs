using FluentValidation;

namespace Logistics.Application.Modules.Operations.Documents.Commands;

internal sealed class CaptureProofOfDeliveryValidator : AbstractValidator<CaptureProofOfDeliveryCommand>
{
    public CaptureProofOfDeliveryValidator()
    {
        RuleFor(x => x.UnableToSignReason).MaximumLength(500);
    }
}
