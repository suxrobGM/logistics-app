using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

internal abstract class ExpenseValidatorBase<T> : AbstractValidator<T> where T : IExpenseFields
{
    protected ExpenseValidatorBase()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.VendorName).MaximumLength(255);
        RuleFor(x => x.ExpenseDate).NotEmpty();
        RuleFor(x => x.ReceiptBlobPath).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
