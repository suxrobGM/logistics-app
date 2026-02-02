using Logistics.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

/// <summary>
///     Extension methods for applying audit field conventions to the model.
/// </summary>
internal static class ModelBuilderAuditExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        ///     Apply audit-field conventions to all entities implementing <see cref="IAuditableEntity" />.
        ///     Call from <c>OnModelCreating</c> after your entity configurations.
        /// </summary>
        public void ApplyAuditableConventions()
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                var entity = modelBuilder.Entity(entityType.ClrType);

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
    }
}
