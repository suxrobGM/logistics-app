using FluentValidation;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

internal sealed class CreateTimeEntryValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.StartedAt).NotEmpty();
        RuleFor(x => x.StartedAt).LessThan(x => x.EndedAt)
            .WithMessage("Start time must be before end time");
        RuleFor(x => x.TotalHours)
            .GreaterThan(0)
            .When(x => x.TotalHours.HasValue)
            .WithMessage("Total hours must be greater than 0");
    }
}
