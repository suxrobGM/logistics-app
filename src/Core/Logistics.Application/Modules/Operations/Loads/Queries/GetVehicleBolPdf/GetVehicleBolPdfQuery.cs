using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>
/// The car hauler electronic BOL for a vehicle load, rendered from its vehicles, condition reports
/// and BOL / POD signatures at the time of the request.
/// </summary>
public class GetVehicleBolPdfQuery : IQuery<Result<VehicleBolPdfResult>>
{
    public Guid LoadId { get; set; }

    /// <summary>
    /// Set by the customer portal: the load must belong to this customer.
    /// </summary>
    public Guid? CustomerId { get; set; }
}

public record VehicleBolPdfResult(byte[] PdfBytes, string FileName);
