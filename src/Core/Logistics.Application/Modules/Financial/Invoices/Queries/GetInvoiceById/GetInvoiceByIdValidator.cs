using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class GetInvoiceByIdValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
