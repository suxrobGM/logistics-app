using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

internal sealed class CreateTruckExpenseValidator : ExpenseValidatorBase<CreateTruckExpenseCommand>
{
    public CreateTruckExpenseValidator()
    {
        RuleFor(x => x.TruckId).NotEmpty();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.OdometerReading).GreaterThanOrEqualTo(0).When(x => x.OdometerReading.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
        RuleFor(x => x.QuantityUnit).IsInEnum().When(x => x.QuantityUnit.HasValue);
    }
}
