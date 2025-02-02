using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateNotificationValidator : AbstractValidator<UpdateNotificationCommand>
{
    public UpdateNotificationValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
