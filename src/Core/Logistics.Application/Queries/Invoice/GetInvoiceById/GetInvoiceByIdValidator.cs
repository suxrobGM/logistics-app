using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetInvoiceByIdValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
