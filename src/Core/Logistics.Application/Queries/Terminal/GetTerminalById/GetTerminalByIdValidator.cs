using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTerminalByIdValidator : AbstractValidator<GetTerminalByIdQuery>
{
    public GetTerminalByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
