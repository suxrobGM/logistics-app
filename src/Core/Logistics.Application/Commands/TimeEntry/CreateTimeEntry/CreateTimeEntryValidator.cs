using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateTimeEntryValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time");
        RuleFor(x => x.TotalHours)
            .GreaterThan(0)
            .When(x => x.TotalHours.HasValue)
            .WithMessage("Total hours must be greater than 0");
    }
}
