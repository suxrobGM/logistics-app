using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdatePayrollInvoiceValidator : AbstractValidator<UpdatePayrollInvoiceCommand>
{
    public UpdatePayrollInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();

        // Only validate period dates when both are provided
        RuleFor(i => i.PeriodStart)
            .LessThan(i => i.PeriodEnd!.Value)
            .When(i => i.PeriodStart.HasValue && i.PeriodEnd.HasValue)
            .WithMessage("Period start must be before period end");
    }
}
