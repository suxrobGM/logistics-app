using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadByIdIdValidator : AbstractValidator<GetLoadByIdQuery>
{
    public GetLoadByIdIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
