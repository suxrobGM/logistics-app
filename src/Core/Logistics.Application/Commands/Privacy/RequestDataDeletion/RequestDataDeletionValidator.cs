using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RequestDataDeletionValidator : AbstractValidator<RequestDataDeletionCommand>
{
    public RequestDataDeletionValidator()
    {
        RuleFor(c => c.Reason).MaximumLength(2048);
    }
}
