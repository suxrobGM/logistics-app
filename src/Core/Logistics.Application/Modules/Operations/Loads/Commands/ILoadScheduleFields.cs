namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>
///     The requested dates and appointment windows shared by create and update, so both validate alike.
/// </summary>
public interface ILoadScheduleFields
{
    DateTime? RequestedPickupDate { get; }
    DateTime? RequestedDeliveryDate { get; }
    DateTime? PickupAppointmentFrom { get; }
    DateTime? PickupAppointmentTo { get; }
    DateTime? DeliveryAppointmentFrom { get; }
    DateTime? DeliveryAppointmentTo { get; }
}
