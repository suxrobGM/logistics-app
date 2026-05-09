using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CancelDataDeletionValidator : AbstractValidator<CancelDataDeletionCommand>
{
    public CancelDataDeletionValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
