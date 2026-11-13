using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Integrations;

/// <summary>
/// Factory for creating accounting provider service instances.
/// </summary>
public interface IAccountingProviderFactory
{
    /// <summary>Get an accounting provider service by type.</summary>
    IAccountingProviderService GetProvider(AccountingProviderType providerType);

    /// <summary>Get a provider service initialized with the given tenant configuration.</summary>
    IAccountingProviderService GetProvider(AccountingProviderConfiguration configuration);

    /// <summary>Check if a provider type is supported.</summary>
    bool IsProviderSupported(AccountingProviderType providerType);
}
