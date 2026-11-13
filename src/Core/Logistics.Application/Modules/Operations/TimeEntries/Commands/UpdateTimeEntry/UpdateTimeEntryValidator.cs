using FluentValidation;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

internal sealed class UpdateTimeEntryValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryValidator()
    {
        RuleFor(x => x.StartedAt)
            .LessThan(x => x.EndedAt!.Value)
            .When(x => x.StartedAt.HasValue && x.EndedAt.HasValue)
            .WithMessage("Start time must be before end time");

        RuleFor(x => x.TotalHours)
            .GreaterThan(0)
            .When(x => x.TotalHours.HasValue)
            .WithMessage("Total hours must be greater than 0");
    }
}
