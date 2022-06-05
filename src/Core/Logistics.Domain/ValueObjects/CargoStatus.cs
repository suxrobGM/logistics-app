namespace Logistics.Domain.ValueObjects;

public class CargoStatus : Enumeration
{
    public CargoStatus(int id, string name) : base(id, name)
    {
    }

    public static CargoStatus Ready = new(1, "ready");
    public static CargoStatus Loaded = new(2, "loaded");
    public static CargoStatus OffDuty = new(3, "offduty");

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
