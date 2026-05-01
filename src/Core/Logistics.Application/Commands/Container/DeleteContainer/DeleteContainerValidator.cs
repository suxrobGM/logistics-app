using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteContainerValidator : AbstractValidator<DeleteContainerCommand>
{
    public DeleteContainerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
