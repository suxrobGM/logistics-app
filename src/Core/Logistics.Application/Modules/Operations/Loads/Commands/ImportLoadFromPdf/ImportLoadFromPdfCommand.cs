using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>
/// Command to import a load from a PDF dispatch sheet. Every import becomes a vehicle load.
/// </summary>
[RequiresFeature(TenantFeature.VehicleTransport)]
public class ImportLoadFromPdfCommand : ICommand<Result<ImportLoadFromPdfResponse>>
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
    /// The optional truck ID to assign to the load.
    /// </summary>
    public Guid? AssignedTruckId { get; set; }
}
