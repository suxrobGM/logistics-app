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

                // Column names are left to the snake_case naming convention
                // (created_at, updated_at, created_by, updated_by).
                entity.Property(nameof(AuditableEntity.CreatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(nameof(AuditableEntity.CreatedBy))
                    .HasMaxLength(50);

                entity.Property(nameof(AuditableEntity.UpdatedBy))
                    .HasMaxLength(50);
            }
        }
    }
}
