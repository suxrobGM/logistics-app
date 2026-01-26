using System.Collections;
using System.Reflection;
using Logistics.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

internal static class ModelBuilderExtensions
{
    private static Type? GetNavigationEntityType(Type type)
    {
        // Single reference navigation.
        if (!typeof(IEnumerable).IsAssignableFrom(type) || type == typeof(string))
        {
            return type.IsClass ? type : null;
        }

        // Collection navigation: List<T>, ICollection<T>, etc.
        return type.IsGenericType ? type.GetGenericArguments()[0] : null;
    }

    /// <summary>
    ///     Returns true if <paramref name="type" /> is a non-abstract configuration type implementing
    ///     <see cref="IEntityTypeConfiguration{TEntity}" /> where <c>TEntity</c> implements
    ///     <paramref name="markerInterface" />.
    /// </summary>
    private static bool IsEligibleConfiguration(Type type, Type markerInterface)
    {
        if (!type.IsClass || type.IsAbstract)
        {
            return false;
        }

        var cfgIf = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

        if (cfgIf is null)
        {
            return false;
        }

        var entityType = cfgIf.GetGenericArguments()[0];
        return markerInterface.IsAssignableFrom(entityType);
    }

    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        ///     Apply audit-field conventions to all entities implementing <see cref="IAuditableEntity" />.
        ///     Call from <c>OnModelCreating</c> after your entity configurations.
        /// </summary>
        private void ApplyAuditableConventions()
        {
            // Snapshot to avoid re-enumeration issues if model changes while iterating.
            var entityTypes = modelBuilder.Model.GetEntityTypes().ToArray();

            foreach (var et in entityTypes)
            {
                if (!typeof(IAuditableEntity).IsAssignableFrom(et.ClrType))
                {
                    continue;
                }

                var entity = modelBuilder.Entity(et.ClrType);

                entity.Property(nameof(AuditableEntity.CreatedAt))
                    .HasColumnName("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(nameof(AuditableEntity.UpdatedAt))
                    .HasColumnName("LastModifiedAt");

                entity.Property(nameof(AuditableEntity.CreatedBy))
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(50);

                entity.Property(nameof(AuditableEntity.UpdatedBy))
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(50);
            }
        }

        /// <summary>
        ///     Apply all <see cref="IEntityTypeConfiguration{TEntity}" /> from the specified assemblies
        ///     where <c>TEntity</c> implements <see cref="ITenantEntity" />.
        /// </summary>
        private void ApplyTenantConfigurationsFromAssembly(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(
                    assembly,
                    type => IsEligibleConfiguration(type, typeof(ITenantEntity)));
            }

            modelBuilder.ApplyAuditableConventions();
        }

        /// <summary>
        ///     Apply all <see cref="IEntityTypeConfiguration{TEntity}" /> from the specified assemblies
        ///     where <c>TEntity</c> implements <see cref="IMasterEntity" />.
        /// </summary>
        private void ApplyMasterConfigurationsFromAssembly(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(
                    assembly,
                    type => IsEligibleConfiguration(type, typeof(IMasterEntity)));
            }

            modelBuilder.ApplyAuditableConventions();
        }

        /// <summary>
        ///     Convenience overload: apply tenant configurations from the assembly that defines <typeparamref name="T" />.
        /// </summary>
        public void ApplyTenantConfigurationsFromAssembly<T>() =>
            modelBuilder.ApplyTenantConfigurationsFromAssembly(typeof(T).Assembly);

        /// <summary>
        ///     Convenience overload: apply master configurations from the assembly that defines <typeparamref name="T" />.
        /// </summary>
        public void ApplyMasterConfigurationsFromAssembly<T>() =>
            modelBuilder.ApplyMasterConfigurationsFromAssembly(typeof(T).Assembly);

        /// <summary>
        ///     For the Master model: remove all tenant-only entity types (implementing <see cref="ITenantEntity" /> but not
        ///     <see cref="IMasterEntity" />) and ignore any navigations on remaining entities that point to those types.
        ///     Call this after applying master configurations.
        /// </summary>
        public void PruneTenantOnlyTypesForMaster()
        {
            // 1) Ignore tenant-only entity types.
            var toIgnore = modelBuilder.Model.GetEntityTypes()
                .Select(et => et.ClrType)
                .Where(t => typeof(ITenantEntity).IsAssignableFrom(t) && !typeof(IMasterEntity).IsAssignableFrom(t))
                .Distinct()
                .ToList();

            foreach (var t in toIgnore)
            {
                modelBuilder.Ignore(t);
            }

            // 2) Remove navigations targeting tenant-only entity types.
            var remaining = modelBuilder.Model.GetEntityTypes()
                .Select(et => et.ClrType)
                .Distinct()
                .ToList();

            foreach (var host in remaining)
            {
                var hostEntity = modelBuilder.Entity(host);

                foreach (var prop in host.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Skip scalar/string.
                    if (prop.PropertyType == typeof(string))
                    {
                        continue;
                    }

                    var navEntityType = GetNavigationEntityType(prop.PropertyType);
                    if (navEntityType is null)
                    {
                        continue;
                    }

                    var isTenantOnly = typeof(ITenantEntity).IsAssignableFrom(navEntityType)
                                       && !typeof(IMasterEntity).IsAssignableFrom(navEntityType);

                    if (isTenantOnly) // Ignore this navigation in the Master model.
                    {
                        hostEntity.Ignore(prop.Name);
                    }
                }
            }
        }
    }
}
