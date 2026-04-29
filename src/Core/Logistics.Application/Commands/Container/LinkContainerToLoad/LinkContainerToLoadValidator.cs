using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class LinkContainerToLoadValidator : AbstractValidator<LinkContainerToLoadCommand>
{
    public LinkContainerToLoadValidator()
    {
        RuleFor(i => i.ContainerId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
    }
}
