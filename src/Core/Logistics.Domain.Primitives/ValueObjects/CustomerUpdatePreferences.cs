using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Which shipment updates a customer gets by email. Everything is on until the carrier turns it off.
/// </summary>
public record CustomerUpdatePreferences
{
    public bool Dispatched { get; set; } = true;
    public bool PickedUp { get; set; } = true;
    public bool Delivered { get; set; } = true;
    public bool Late { get; set; } = true;

    /// <summary>
    /// Whether the customer wants this update. A dispatcher-approved delay notice always goes out.
    /// </summary>
    public bool Wants(CustomerUpdateEvent updateEvent) => updateEvent switch
    {
        CustomerUpdateEvent.Dispatched => Dispatched,
        CustomerUpdateEvent.PickedUp => PickedUp,
        CustomerUpdateEvent.Delivered => Delivered,
        CustomerUpdateEvent.Late => Late,
        _ => true
    };
}
