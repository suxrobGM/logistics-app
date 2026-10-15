using FluentValidation;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal abstract class AccessorialChargeValidatorBase<T> : AbstractValidator<T> where T : IAccessorialChargeFields
{
    protected AccessorialChargeValidatorBase()
    {
        RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
        RuleFor(i => i.LineItemType).IsInEnum();
        RuleFor(i => i.Unit).IsInEnum();
        RuleFor(i => i.DefaultAmount).GreaterThanOrEqualTo(0);
    }
}
