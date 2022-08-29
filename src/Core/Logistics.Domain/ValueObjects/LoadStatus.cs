namespace Logistics.Domain.ValueObjects;

public class LoadStatus : Enumeration
{
    public LoadStatus(int id, string name) : base(id, name)
    {
    }

    public static readonly LoadStatus Dispatched = new(1, "dispatched");
    public static readonly LoadStatus PickedUp = new(2, "pickedup");
    public static readonly LoadStatus Delivered = new(3, "delivered");

    public static LoadStatus? Get(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        
        name = name.Trim().ToLower();
        return name switch
        {
            "dispatched" => Dispatched,
            "pickedup" => PickedUp,
            "delivered" => Delivered,
            _ => null
        };
    }
}
