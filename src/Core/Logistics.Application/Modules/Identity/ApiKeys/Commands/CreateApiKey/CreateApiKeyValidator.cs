using FluentValidation;

namespace Logistics.Application.Modules.Identity.ApiKeys.Commands;

internal sealed class CreateApiKeyValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
