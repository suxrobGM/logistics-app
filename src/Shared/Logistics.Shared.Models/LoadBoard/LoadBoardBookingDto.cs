namespace Logistics.Shared.Models;

public record LoadBoardBookingRequest
{
    /// <summary>
    /// Truck to assign to the load
    /// </summary>
    public Guid TruckId { get; set; }

    /// <summary>
    /// Dispatcher to assign to the load
    /// </summary>
    public Guid DispatcherId { get; set; }

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
}

public record LoadBoardBookingResultDto
{
    public bool Success { get; set; }

    /// <summary>
    /// Confirmation ID from the load board provider
    /// </summary>
    public string? ExternalConfirmationId { get; set; }

    /// <summary>
    /// Error message if booking failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The created Load ID if successful
    /// </summary>
    public Guid? CreatedLoadId { get; set; }

    /// <summary>
    /// The created Load number if successful
    /// </summary>
    public long? CreatedLoadNumber { get; set; }
}
