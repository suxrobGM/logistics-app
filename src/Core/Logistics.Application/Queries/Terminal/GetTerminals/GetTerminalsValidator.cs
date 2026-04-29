using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTerminalsValidator : AbstractValidator<GetTerminalsQuery>
{
    public GetTerminalsValidator()
    {
        RuleFor(i => i.Page).GreaterThan(0);
        RuleFor(i => i.PageSize).GreaterThan(0);
    }
}
