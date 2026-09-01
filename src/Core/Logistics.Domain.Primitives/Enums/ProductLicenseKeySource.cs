namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Where the active license key was read from. Configuration wins over the database.
/// </summary>
public enum ProductLicenseKeySource
{
    None,
    Configuration,
    SystemSettings
}
