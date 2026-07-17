using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.FuelCards;

/// <summary>
/// Interface for fuel card provider integrations.
/// Each provider (WEX, EFS, Comdata) implements this interface.
/// </summary>
public interface IFuelCardProviderService
{
    /// <summary>
    /// The type of fuel card provider this service handles
    /// </summary>
    FuelCardProviderType ProviderType { get; }

    /// <summary>
    /// Initialize the service with credentials from the configuration
    /// </summary>
    void Initialize(FuelCardProviderConfiguration configuration);

    /// <summary>
    /// Validate that the provided credentials are correct
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret);

    /// <summary>
    /// Refresh an OAuth access token using a refresh token
    /// </summary>
    Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Pull card transactions posted since the given UTC timestamp.
    /// </summary>
    Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(DateTime sinceUtc, CancellationToken ct = default);
}
