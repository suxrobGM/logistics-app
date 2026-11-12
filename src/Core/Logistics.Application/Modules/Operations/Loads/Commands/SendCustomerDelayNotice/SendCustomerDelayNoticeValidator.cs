using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class SendCustomerDelayNoticeValidator : AbstractValidator<SendCustomerDelayNoticeCommand>
{
    public const int MaxMessageLength = 1000;

    public SendCustomerDelayNoticeValidator()
    {
        RuleFor(i => i.Message).NotEmpty().MaximumLength(MaxMessageLength);
    }
}
