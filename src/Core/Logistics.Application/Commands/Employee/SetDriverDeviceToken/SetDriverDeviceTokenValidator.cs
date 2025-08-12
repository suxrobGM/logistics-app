using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class SetDriverDeviceTokenValidator : AbstractValidator<SetDriverDeviceTokenCommand>
{
    public SetDriverDeviceTokenValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        RuleFor(i => i.DeviceToken).NotEmpty();
    }
}
