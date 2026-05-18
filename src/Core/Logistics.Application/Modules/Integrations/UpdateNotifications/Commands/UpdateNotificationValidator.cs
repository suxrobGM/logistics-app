using FluentValidation;

namespace Logistics.Application.Modules.Integrations.UpdateNotifications.Commands;

internal sealed class UpdateNotificationValidator : AbstractValidator<UpdateNotificationCommand>
{
    public UpdateNotificationValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
