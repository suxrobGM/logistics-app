using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class UpdateLoadChargesValidator : AbstractValidator<UpdateLoadCommand>
{
    public UpdateLoadChargesValidator()
    {
        RuleFor(i => i.FuelSurcharge).GreaterThanOrEqualTo(0);
        RuleForEach(i => i.Accessorials).SetValidator(new LoadAccessorialValidator());
        RuleFor(i => i.NmfcCode).MaximumLength(20);
        RuleFor(i => i.Dimensions).MaximumLength(100);
        RuleFor(i => i.PalletCount).GreaterThanOrEqualTo(0);
        RuleFor(i => i.PieceCount).GreaterThanOrEqualTo(0);
        RuleFor(i => i.Weight).GreaterThanOrEqualTo(0);
    }
}
