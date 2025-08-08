using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UploadLoadDocumentValidator : AbstractValidator<UploadLoadDocumentCommand>
{
    public UploadLoadDocumentValidator()
    {
        RuleFor(x => x.LoadId)
            .NotEmpty()
            .WithMessage("Load ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeValidContentType)
            .WithMessage("Invalid content type. Only documents, images, and PDFs are allowed");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(50 * 1024 * 1024) // 50MB
            .WithMessage("File size cannot exceed 50MB");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid document type");

        RuleFor(x => x.UploadedById)
            .NotEmpty()
            .WithMessage("Uploader ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.FileContent)
            .NotNull()
            .WithMessage("File content is required");
    }

    private static bool BeValidContentType(string contentType)
    {
        var allowedTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain",
            "text/csv",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        return allowedTypes.Contains(contentType.ToLowerInvariant());
    }
}