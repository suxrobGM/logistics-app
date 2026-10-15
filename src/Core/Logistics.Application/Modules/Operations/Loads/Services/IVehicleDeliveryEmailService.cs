using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>
///     Tells the customer a car hauler load was delivered and links the shipment page, where the
///     electronic BOL can be downloaded.
/// </summary>
public interface IVehicleDeliveryEmailService : IApplicationService
{
    /// <summary>
    ///     Sends the email when <paramref name="load"/> is a delivered vehicle load whose customer has an
    ///     email address. Never throws: a failed email must not fail the delivery.
    /// </summary>
    Task SendAsync(Load load, CancellationToken ct = default);
}
