using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Services;

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
    /// Get a load board provider service initialized with the given configuration
    /// </summary>
    ILoadBoardProviderService GetProvider(LoadBoardConfiguration configuration);

    /// <summary>
    /// Check if a provider type is supported
    /// </summary>
    bool IsProviderSupported(LoadBoardProviderType providerType);
}
