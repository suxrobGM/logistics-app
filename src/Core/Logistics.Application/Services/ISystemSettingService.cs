namespace Logistics.Application.Services;

/// <summary>
/// Service for reading and writing system-wide settings stored in the master database.
/// </summary>
public interface ISystemSettingService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, string? description = null, CancellationToken ct = default);
}
