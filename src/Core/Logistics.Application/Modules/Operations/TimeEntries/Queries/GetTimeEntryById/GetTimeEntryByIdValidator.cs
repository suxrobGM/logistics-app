using FluentValidation;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

internal sealed class GetTimeEntryByIdValidator : AbstractValidator<GetTimeEntryByIdQuery>
{
    public GetTimeEntryByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
