using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

internal sealed class CancelDataDeletionValidator : AbstractValidator<CancelDataDeletionCommand>
{
    public CancelDataDeletionValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
