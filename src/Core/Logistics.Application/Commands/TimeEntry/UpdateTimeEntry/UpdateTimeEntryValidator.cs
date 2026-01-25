using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateTimeEntryValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime!.Value)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("Start time must be before end time");

        RuleFor(x => x.TotalHours)
            .GreaterThan(0)
            .When(x => x.TotalHours.HasValue)
            .WithMessage("Total hours must be greater than 0");
    }
}
