using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTimeEntryByIdValidator : AbstractValidator<GetTimeEntryByIdQuery>
{
    public GetTimeEntryByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
