using FluentValidation;

namespace Logistics.Application.Commands;

public class ImportLoadFromPdfValidator : AbstractValidator<ImportLoadFromPdfCommand>
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ImportLoadFromPdfValidator()
    {
        RuleFor(x => x.PdfContent)
            .NotNull()
            .WithMessage("PDF file is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(name => name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File must be a PDF");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty()
            .WithMessage("Current user ID is required");
    }
}
