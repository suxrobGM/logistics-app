using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class BookLoadBoardLoadCommand : IAppRequest<Result<LoadBoardBookingResultDto>>
{
    /// <summary>
    /// ID of the load board listing to book
    /// </summary>
    public Guid ListingId { get; set; }

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
