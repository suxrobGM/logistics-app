namespace Logistics.Domain.ValueObjects;

public class CargoStatus : Enumeration
{
    public CargoStatus(int id, string name) : base(id, name)
    {
    }

    public static readonly CargoStatus Ready = new(1, "ready");
    public static readonly CargoStatus Loaded = new(2, "loaded");
    public static readonly CargoStatus OffDuty = new(3, "offduty");

    public static CargoStatus Get(string name)
    {
        name = name.Trim().ToLower();
        return name switch
        {
            "ready" => Ready,
            "loaded" => Loaded,
            "offduty" => OffDuty,
            _ => throw new InvalidOperationException($"Could not found the corresponding enum type for the '{name}'"),
        };
    }
}
