using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// Factory for creating load board provider service instances
/// </summary>
public interface ILoadBoardProviderFactory
{
    /// <summary>
    /// Get a load board provider service by type
    /// </summary>
    ILoadBoardProviderService GetProvider(LoadBoardProviderType providerType);

    /// <summary>
    /// Check if a provider type is supported
    /// </summary>
    bool IsProviderSupported(LoadBoardProviderType providerType);
}
