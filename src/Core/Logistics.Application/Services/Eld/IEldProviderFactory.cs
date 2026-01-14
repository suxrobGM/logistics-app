using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Services;

/// <summary>
/// Factory for creating ELD provider service instances
/// </summary>
public interface IEldProviderFactory
{
    /// <summary>
    /// Get an ELD provider service by type
    /// </summary>
    IEldProviderService GetProvider(EldProviderType providerType);

    /// <summary>
    /// Get an ELD provider service initialized with the given configuration
    /// </summary>
    IEldProviderService GetProvider(EldProviderConfiguration configuration);

    /// <summary>
    /// Check if a provider type is supported
    /// </summary>
    bool IsProviderSupported(EldProviderType providerType);
}
