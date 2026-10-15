using FluentValidation;

namespace Logistics.Application.Modules.Platform.DataImport.Commands;

/// <summary>
///     Extension allowlist plus a content type that must agree with the extension - the extension alone
///     is not a check. The file is parsed in memory and never stored.
/// </summary>
internal abstract class ImportFileValidatorBase<T> : AbstractValidator<T> where T : IImportFile
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Windows browsers report CSV as application/vnd.ms-excel when Excel is installed.
        [".csv"] = ["text/csv", "application/csv", "text/plain", "text/comma-separated-values", "application/vnd.ms-excel"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]
    };

    protected ImportFileValidatorBase()
    {
        RuleFor(i => i.EntityType).IsInEnum();

        RuleFor(i => i.FileName)
            .NotEmpty()
            .Must(name => AllowedContentTypes.ContainsKey(Path.GetExtension(name)))
            .WithMessage("Upload a .csv or .xlsx file.");

        RuleFor(i => i)
            .Must(i => AllowedContentTypes.TryGetValue(Path.GetExtension(i.FileName), out var types) &&
                       types.Contains(StripParameters(i.ContentType), StringComparer.OrdinalIgnoreCase))
            .WithName("ContentType")
            .WithMessage("The file's content type does not match its extension.");

        RuleFor(i => i.FileSizeBytes)
            .GreaterThan(0).WithMessage("The file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("The file exceeds the 10 MB limit.");
    }

    private static string StripParameters(string contentType) => contentType.Split(';')[0].Trim();
}
