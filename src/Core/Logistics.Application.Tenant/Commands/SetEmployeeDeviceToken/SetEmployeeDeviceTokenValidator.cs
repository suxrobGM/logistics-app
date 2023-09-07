using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class SetEmployeeDeviceTokenValidator : AbstractValidator<SetEmployeeDeviceTokenCommand>
{
    public SetEmployeeDeviceTokenValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        RuleFor(i => i.DeviceToken).NotEmpty();
    }
}
