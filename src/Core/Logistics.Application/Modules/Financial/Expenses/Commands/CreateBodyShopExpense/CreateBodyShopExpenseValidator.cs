using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

internal sealed class CreateBodyShopExpenseValidator : ExpenseValidatorBase<CreateBodyShopExpenseCommand>
{
    public CreateBodyShopExpenseValidator()
    {
        RuleFor(x => x.TruckId).NotEmpty();
        RuleFor(x => x.VendorAddress).MaximumLength(500);
        RuleFor(x => x.VendorPhone).MaximumLength(20);
        RuleFor(x => x.RepairDescription).MaximumLength(2000);
    }
}
