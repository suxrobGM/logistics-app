using FluentValidation;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class UpdateCustomerDetentionTermsValidator : AbstractValidator<UpdateCustomerDetentionTermsCommand>
{
    public UpdateCustomerDetentionTermsValidator()
    {
        RuleFor(i => i.FreeMinutes).InclusiveBetween(0, 24 * 60);
        RuleFor(i => i.RatePerHour).GreaterThanOrEqualTo(0);
    }
}
