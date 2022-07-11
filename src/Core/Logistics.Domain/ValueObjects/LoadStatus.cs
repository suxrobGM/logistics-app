namespace Logistics.Domain.ValueObjects;

public class LoadStatus : Enumeration
{
    public LoadStatus(int id, string name) : base(id, name)
    {
    }

    public static readonly LoadStatus Ready = new(1, "ready");
    public static readonly LoadStatus Loaded = new(2, "loaded");
    public static readonly LoadStatus OffDuty = new(3, "offduty");

    public static LoadStatus Get(string name)
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
