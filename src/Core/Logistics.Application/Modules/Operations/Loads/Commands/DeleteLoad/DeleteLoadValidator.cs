using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class DeleteLoadValidator : AbstractValidator<DeleteLoadCommand>
{
    public DeleteLoadValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
