using System.Reflection;
using Logistics.Mediator;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the mediator and discovers handlers. Pipeline behaviours are registered directly,
/// outermost first: <c>services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(MyBehaviour&lt;,&gt;))</c>.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IMediator" /> without scanning. Use in a host that dispatches requests
    /// but owns none of the handlers.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // TryAdd, because a host may call this once per layer it composes.
        services.TryAddTransient<IMediator, global::Logistics.Mediator.Mediator>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="IMediator" /> and every handler in <paramref name="assembly" />.
    /// </summary>
    /// <param name="typeFilter">
    /// Narrows the scan. A host that composes only one slice of an assembly must pass this, or it
    /// registers handlers whose dependencies it never wires.
    /// </param>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Assembly assembly,
        Func<Type, bool>? typeFilter = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        services.AddMediator();

        var requestHandlers = new Dictionary<Type, Type>();

        foreach (var type in GetLoadableTypes(assembly))
        {
            if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
            {
                continue;
            }

            if (typeFilter?.Invoke(type) == false)
            {
                continue;
            }

            foreach (var contract in type.GetInterfaces())
            {
                if (!contract.IsGenericType)
                {
                    continue;
                }

                var definition = contract.GetGenericTypeDefinition();

                if (definition == typeof(IRequestHandler<,>))
                {
                    // Without this, a duplicate silently wins: GetService returns the last one.
                    if (requestHandlers.TryGetValue(contract, out var existing) && existing != type)
                    {
                        throw new InvalidOperationException(
                            $"Two handlers are registered for '{contract}': '{existing}' and '{type}'. " +
                            "A request may have exactly one handler.");
                    }

                    requestHandlers[contract] = type;
                }
                else if (definition == typeof(INotificationHandler<>))
                {
                    // Dedupes on (service, implementation), so repeat calls are idempotent while
                    // distinct notification handlers for one event all register.
                    services.TryAddEnumerable(new ServiceDescriptor(contract, type, ServiceLifetime.Transient));
                }
            }
        }

        foreach (var (contract, implementation) in requestHandlers)
        {
            services.TryAdd(new ServiceDescriptor(contract, implementation, ServiceLifetime.Transient));
        }

        return services;
    }

    // The Application assembly pulls Stripe, EF Core, FluentValidation and Mapperly; a partially
    // restored context can fault the load rather than return nothing.
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
