using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPayrollByIdValidator : AbstractValidator<GetPayrollByIdQuery>
{
    public GetPayrollByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
