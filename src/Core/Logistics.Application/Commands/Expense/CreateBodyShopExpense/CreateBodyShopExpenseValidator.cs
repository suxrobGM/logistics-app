using FluentValidation;

namespace Logistics.Application.Commands;

public class CreateBodyShopExpenseValidator : AbstractValidator<CreateBodyShopExpenseCommand>
{
    public CreateBodyShopExpenseValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.VendorName).MaximumLength(255);
        RuleFor(x => x.ExpenseDate).NotEmpty();
        RuleFor(x => x.ReceiptBlobPath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.TruckId).NotEmpty();
        RuleFor(x => x.VendorAddress).MaximumLength(500);
        RuleFor(x => x.VendorPhone).MaximumLength(20);
        RuleFor(x => x.RepairDescription).MaximumLength(2000);
    }
}
