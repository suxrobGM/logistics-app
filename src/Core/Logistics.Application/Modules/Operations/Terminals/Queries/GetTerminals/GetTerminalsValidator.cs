using FluentValidation;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

internal sealed class GetTerminalsValidator : AbstractValidator<GetTerminalsQuery>
{
    public GetTerminalsValidator()
    {
        RuleFor(i => i.Page).GreaterThan(0);
        RuleFor(i => i.PageSize).GreaterThan(0);
    }
}
