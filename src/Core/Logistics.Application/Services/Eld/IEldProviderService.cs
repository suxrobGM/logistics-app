using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Services;

/// <summary>
/// Interface for ELD provider integrations.
/// Each ELD provider (Samsara, Motive, etc.) implements this interface.
/// </summary>
public interface IEldProviderService
{
    /// <summary>
    /// The type of ELD provider this service handles
    /// </summary>
    EldProviderType ProviderType { get; }

    /// <summary>
    /// Initialize the service with credentials from the configuration
    /// </summary>
    void Initialize(EldProviderConfiguration configuration);

    /// <summary>
    /// Validate that the provided credentials are correct
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret);

    /// <summary>
    /// Refresh an OAuth access token using a refresh token
    /// </summary>
    Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Get current HOS status for a specific driver
    /// </summary>
    Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId);

    /// <summary>
    /// Get current HOS status for all drivers in the account
    /// </summary>
    Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync();

    /// <summary>
    /// Get historical HOS logs for a driver within a date range
    /// </summary>
    Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get HOS violations for a driver within a date range
    /// </summary>
    Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get all drivers from the ELD provider for mapping
    /// </summary>
    Task<IEnumerable<EldDriverDto>> GetAllDriversAsync();

    /// <summary>
    /// Get all vehicles from the ELD provider for mapping
    /// </summary>
    Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync();

    /// <summary>
    /// Process an incoming webhook from the ELD provider
    /// </summary>
    Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret);
}
