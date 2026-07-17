using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

internal sealed class UpdateExpenseValidator : ExpenseValidatorBase<UpdateExpenseCommand>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.OdometerReading).GreaterThanOrEqualTo(0).When(x => x.OdometerReading.HasValue);
        RuleFor(x => x.VendorAddress).MaximumLength(500);
        RuleFor(x => x.VendorPhone).MaximumLength(20);
        RuleFor(x => x.RepairDescription).MaximumLength(2000);
    }
}
