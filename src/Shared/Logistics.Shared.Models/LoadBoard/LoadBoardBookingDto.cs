namespace Logistics.Shared.Models;

public record LoadBoardBookingRequest
{
    /// <summary>
    /// Truck to assign to the load
    /// </summary>
    public Guid TruckId { get; set; }

    /// <summary>
    /// Dispatcher to assign to the load; the signed-in user when omitted
    /// </summary>
    public Guid? DispatcherId { get; set; }

    /// <summary>
    /// Customer ID (if existing customer)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Customer name (if creating new customer from broker info)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Optional notes for the booking
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Dispatcher override: book even when the broker's credit is below the tenant's
    /// minimum score or the broker's authority is inactive.
    /// </summary>
    public bool OverrideCreditCheck { get; set; }
}

/// <summary>
/// The load created from a listing. No load board books through its API, so the broker has not
/// been told: the dispatcher confirms with the broker contact returned here.
/// </summary>
public record LoadBoardBookingResultDto
{
    public Guid CreatedLoadId { get; set; }
    public long CreatedLoadNumber { get; set; }

    public string? BrokerName { get; set; }
    public string? BrokerPhone { get; set; }
    public string? BrokerEmail { get; set; }
}
