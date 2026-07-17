using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Shared base for the third-party provider factories (ELD, fuel card, load board, accounting).
/// A subclass declares its provider-enum → implementation-type map and family name; this base
/// resolves the concrete provider service from the container and reports support.
///
/// Per-family "initialize from a tenant configuration" overloads stay on the subclass, since the
/// configuration types differ per family.
/// </summary>
/// <typeparam name="TService">The provider service abstraction (e.g. <c>IEldProviderService</c>).</typeparam>
/// <typeparam name="TProviderType">The provider-type enum (e.g. <c>EldProviderType</c>).</typeparam>
public abstract class ProviderFactoryBase<TService, TProviderType>(
    IServiceProvider serviceProvider, ILogger logger)
    where TService : class
    where TProviderType : struct, Enum
{
    protected abstract IReadOnlyDictionary<TProviderType, Type> Providers { get; }
    protected abstract string FamilyName { get; }

    public TService GetProvider(TProviderType providerType)
    {
        if (!Providers.TryGetValue(providerType, out var implementationType))
            throw new NotSupportedException($"{FamilyName} provider '{providerType}' is not supported");
        var service = (TService)serviceProvider.GetRequiredService(implementationType);
        logger.LogDebug("Created {Family} provider service for {ProviderType}", FamilyName, providerType);
        return service;
    }

    public bool IsProviderSupported(TProviderType providerType) => Providers.ContainsKey(providerType);
}
