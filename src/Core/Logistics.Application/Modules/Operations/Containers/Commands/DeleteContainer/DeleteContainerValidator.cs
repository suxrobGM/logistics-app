using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class DeleteContainerValidator : AbstractValidator<DeleteContainerCommand>
{
    public DeleteContainerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
