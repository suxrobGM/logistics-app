using FluentValidation;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

internal sealed class GetTerminalByIdValidator : AbstractValidator<GetTerminalByIdQuery>
{
    public GetTerminalByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
