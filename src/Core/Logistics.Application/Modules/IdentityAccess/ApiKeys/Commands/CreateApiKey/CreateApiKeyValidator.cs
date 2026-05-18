using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.ApiKeys.Commands;

internal sealed class CreateApiKeyValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
