using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UploadTenantLogoValidator : AbstractValidator<UploadTenantLogoCommand>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".webp", ".gif"];

    public UploadTenantLogoValidator()
    {
        RuleFor(i => i.TenantId)
            .NotEmpty();

        RuleFor(i => i.FileContent)
            .NotNull();

        RuleFor(i => i.FileName)
            .NotEmpty()
            .Must(HaveAnAllowedExtension)
            .WithMessage("Logo must be a PNG, JPG, WEBP, or GIF image.");

        RuleFor(i => i.ContentType)
            .NotEmpty()
            .Must(BeAnImage)
            .WithMessage("File must be an image")
            // SVG can carry script, so it is refused even though it is an image type.
            .Must(contentType => !contentType.Contains("svg", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SVG images are not allowed for logos.");

        RuleFor(i => i.FileSizeBytes)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("File size exceeds the maximum allowed (5 MB)");
    }

    private static bool HaveAnAllowedExtension(string fileName)
    {
        return AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
    }

    private static bool BeAnImage(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
