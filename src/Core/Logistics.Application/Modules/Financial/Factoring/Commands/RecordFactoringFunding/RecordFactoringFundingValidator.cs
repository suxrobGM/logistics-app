using FluentValidation;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class RecordFactoringFundingValidator : AbstractValidator<RecordFactoringFundingCommand>
{
    public RecordFactoringFundingValidator()
    {
        RuleFor(x => x.Invoices).NotEmpty();
        RuleFor(x => x.FundedAt).NotEmpty();
        RuleForEach(x => x.Invoices).ChildRules(line =>
        {
            line.RuleFor(l => l.AdvanceAmount).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.FeeAmount).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.RejectionReason).MaximumLength(1000);
        });
    }
}
