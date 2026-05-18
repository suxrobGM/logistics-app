using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UploadTenantLogoValidator : AbstractValidator<UploadTenantLogoCommand>
{
    public UploadTenantLogoValidator()
    {
        RuleFor(i => i.TenantId)
            .NotEmpty();

        RuleFor(i => i.FileContent)
            .NotNull();

        RuleFor(i => i.FileName)
            .NotEmpty();

        RuleFor(i => i.ContentType)
            .NotEmpty();
    }
}
