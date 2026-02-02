using Logistics.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

/// <summary>
///     Extension methods for applying entity configurations to the EF Core model
///     based on marker interfaces (IMasterEntity, ITenantEntity).
/// </summary>
internal static class ModelBuilderExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        ///     Apply all <see cref="IEntityTypeConfiguration{TEntity}" /> from the assembly defining <typeparamref name="T" />
        ///     where <c>TEntity</c> implements <see cref="ITenantEntity" />.
        /// </summary>
        public void ApplyTenantConfigurationsFromAssembly<T>()
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(T).Assembly,
                type => IsConfigurationFor<ITenantEntity>(type));

            modelBuilder.ApplyAuditableConventions();
        }

        /// <summary>
        ///     Apply all <see cref="IEntityTypeConfiguration{TEntity}" /> from the assembly defining <typeparamref name="T" />
        ///     where <c>TEntity</c> implements <see cref="IMasterEntity" />.
        /// </summary>
        public void ApplyMasterConfigurationsFromAssembly<T>()
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(T).Assembly,
                type => IsConfigurationFor<IMasterEntity>(type));

            modelBuilder.ApplyAuditableConventions();
        }
    }

    /// <summary>
    ///     Returns true if <paramref name="type" /> is a non-abstract configuration class implementing
    ///     <see cref="IEntityTypeConfiguration{TEntity}" /> where <c>TEntity</c> implements <typeparamref name="TMarker" />.
    /// </summary>
    private static bool IsConfigurationFor<TMarker>(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
        {
            return false;
        }

        var configInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

        if (configInterface is null)
        {
            return false;
        }

        var entityType = configInterface.GetGenericArguments()[0];
        return typeof(TMarker).IsAssignableFrom(entityType);
    }
}
