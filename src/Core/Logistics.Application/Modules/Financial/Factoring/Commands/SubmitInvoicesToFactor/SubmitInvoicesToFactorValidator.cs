using FluentValidation;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class SubmitInvoicesToFactorValidator : AbstractValidator<SubmitInvoicesToFactorCommand>
{
    public SubmitInvoicesToFactorValidator()
    {
        RuleFor(x => x.InvoiceIds).NotEmpty().WithMessage("Select at least one invoice to submit.");
        RuleFor(x => x.InvoiceIds.Count).LessThanOrEqualTo(100).WithName("Invoices");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
