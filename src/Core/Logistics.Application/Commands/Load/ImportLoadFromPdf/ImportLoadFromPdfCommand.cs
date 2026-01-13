using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to import a load from a PDF dispatch sheet.
/// </summary>
public class ImportLoadFromPdfCommand : IAppRequest<Result<ImportLoadFromPdfResponse>>
{
    /// <summary>
    /// The PDF file stream.
    /// </summary>
    public required Stream PdfContent { get; set; }

    /// <summary>
    /// The original file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// The ID of the current user (will be set as dispatcher).
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// The truck ID to assign to the load.
    /// </summary>
    public required Guid AssignedTruckId { get; set; }
}
