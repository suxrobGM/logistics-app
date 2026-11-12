using System.Reflection;
using Logistics.Mediator;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Register pipeline behaviours yourself as open-generic transients, outermost first.</summary>
public static class MediatorServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>For a host that dispatches requests but owns none of the handlers.</summary>
        public IServiceCollection AddMediator()
        {
            ArgumentNullException.ThrowIfNull(services);

            // TryAdd, because a host may call this once per layer it composes.
            services.TryAddTransient<IMediator, global::Logistics.Mediator.Mediator>();
            return services;
        }

        /// <summary>A host composing one slice must pass <paramref name="typeFilter"/>, or it registers handlers it can't build.</summary>
        public IServiceCollection AddMediator(
            Assembly assembly,
            Func<Type, bool>? typeFilter = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assembly);

            services.AddMediator();

            var requestHandlers = new Dictionary<Type, Type>();

            foreach (var type in assembly.GetTypes())
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
    }

}
