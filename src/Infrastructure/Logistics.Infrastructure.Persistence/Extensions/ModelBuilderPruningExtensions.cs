using System.Collections;
using System.Reflection;
using Logistics.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

/// <summary>
///     Extension methods for pruning entity types from the EF Core model
///     based on marker interfaces (IMasterEntity, ITenantEntity).
/// </summary>
internal static class ModelBuilderPruningExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        ///     For the Tenant model: remove all master-only entity types (implementing <see cref="IMasterEntity" /> but not
        ///     <see cref="ITenantEntity" />) and ignore any navigations pointing to those types.
        /// </summary>
        public void PruneMasterOnlyTypesForTenant() =>
            modelBuilder.PruneEntityTypes<IMasterEntity, ITenantEntity>();

        /// <summary>
        ///     For the Master model: remove all tenant-only entity types (implementing <see cref="ITenantEntity" /> but not
        ///     <see cref="IMasterEntity" />) and ignore any navigations pointing to those types.
        /// </summary>
        public void PruneTenantOnlyTypesForMaster() =>
            modelBuilder.PruneEntityTypes<ITenantEntity, IMasterEntity>();

        /// <summary>
        ///     Generic pruning: removes entity types implementing <typeparamref name="TExclude" /> but not
        ///     <typeparamref name="TKeep" />, and ignores navigations pointing to those types.
        /// </summary>
        private void PruneEntityTypes<TExclude, TKeep>()
        {
            var typesToIgnore = modelBuilder.Model.GetEntityTypes()
                .Select(et => et.ClrType)
                .Where(t => typeof(TExclude).IsAssignableFrom(t) && !typeof(TKeep).IsAssignableFrom(t))
                .ToHashSet();

            // 1) Ignore the entity types themselves.
            foreach (var type in typesToIgnore)
            {
                modelBuilder.Ignore(type);
            }

            // 2) Ignore navigations pointing to excluded types.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var hostEntity = modelBuilder.Entity(entityType.ClrType);

                foreach (var prop in entityType.ClrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var navType = GetNavigationTargetType(prop.PropertyType);

                    if (navType is not null && typesToIgnore.Contains(navType))
                    {
                        hostEntity.Ignore(prop.Name);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Extracts the target entity type from a navigation property type.
    ///     Returns null for non-navigation types (primitives, strings, value types).
    /// </summary>
    private static Type? GetNavigationTargetType(Type propertyType)
    {
        if (propertyType == typeof(string) || propertyType.IsValueType)
        {
            return null;
        }

        // Collection navigation: IEnumerable<T>, List<T>, ICollection<T>, etc.
        if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType.IsGenericType)
        {
            return propertyType.GetGenericArguments()[0];
        }

        // Single reference navigation.
        return propertyType.IsClass ? propertyType : null;
    }
}
