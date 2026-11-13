namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Damage types printed on car hauler bills of lading. Each has the short industry code drivers
/// write on the vehicle diagram (S, D, C, CR, M, B) - see <c>VehicleDamageExtensions.GetCode</c>.
/// </summary>
public enum VehicleDamageType
{
    Scratch,
    Dent,
    Chip,
    Crack,
    Missing,
    Broken
}
