using Logistics.Domain.Core;
using Logistics.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

internal static class ModelBuilderExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        public void ApplyTenantConfigurationsFromAssembly<T>()
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(T).Assembly,
                type => IsConfigurationFor<ITenantEntity>(type));

            modelBuilder.ApplyAuditableConventions();
        }

        public void ApplyMasterConfigurationsFromAssembly<T>()
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(T).Assembly,
                type => IsConfigurationFor<IMasterEntity>(type));

            modelBuilder.ApplyAuditableConventions();
        }
    }

    extension(ModelConfigurationBuilder configurationBuilder)
    {
        /// <summary>Conventions shared by the master and tenant contexts.</summary>
        public void AddLogisticsConventions()
        {
            configurationBuilder.Conventions.Add(_ => new SnakeCaseEnumConvention());
            configurationBuilder.Conventions.Add(_ => new IsoCodeLengthConvention());
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }
    }

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
