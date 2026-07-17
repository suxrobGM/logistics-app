using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.FuelCards;

/// <summary>
/// Factory for creating fuel card provider service instances
/// </summary>
public interface IFuelCardProviderFactory
{
    /// <summary>
    /// Get a fuel card provider service by type
    /// </summary>
    IFuelCardProviderService GetProvider(FuelCardProviderType providerType);

    /// <summary>
    /// Get a fuel card provider service initialized with the given configuration
    /// </summary>
    IFuelCardProviderService GetProvider(FuelCardProviderConfiguration configuration);

    /// <summary>
    /// Check if a provider type is supported
    /// </summary>
    bool IsProviderSupported(FuelCardProviderType providerType);
}
