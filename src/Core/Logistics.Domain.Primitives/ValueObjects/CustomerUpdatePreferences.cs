using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>Everything is on until the carrier turns it off.</summary>
public record CustomerUpdatePreferences
{
    public bool Dispatched { get; set; } = true;
    public bool PickedUp { get; set; } = true;
    public bool Delivered { get; set; } = true;
    public bool Late { get; set; } = true;

    /// <summary>A dispatcher-approved delay notice always goes out.</summary>
    public bool Wants(CustomerUpdateEvent updateEvent) => updateEvent switch
    {
        CustomerUpdateEvent.Dispatched => Dispatched,
        CustomerUpdateEvent.PickedUp => PickedUp,
        CustomerUpdateEvent.Delivered => Delivered,
        CustomerUpdateEvent.Late => Late,
        _ => true
    };
}
