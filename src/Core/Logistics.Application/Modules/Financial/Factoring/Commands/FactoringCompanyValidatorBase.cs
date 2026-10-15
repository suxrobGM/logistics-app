using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal abstract class FactoringCompanyValidatorBase<T> : AbstractValidator<T> where T : IFactoringCompanyFields
{
    protected FactoringCompanyValidatorBase()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SubmissionEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.AdvanceRatePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.FeePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.AdvanceRatePercent + x.FeePercent)
            .LessThanOrEqualTo(100)
            .WithName("Advance rate plus fee")
            .WithMessage("The advance rate and the fee together cannot exceed 100%.");

        RuleFor(x => x.RemitToAddress!)
            .SetValidator(new AddressValidator())
            .When(x => x.RemitToAddress is not null);
    }
}
