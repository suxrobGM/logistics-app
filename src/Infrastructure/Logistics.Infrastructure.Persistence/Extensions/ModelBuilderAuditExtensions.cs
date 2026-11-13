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
        /// <summary>Applies the auditable-entity conventions to every entity type. Call it in <c>OnModelCreating</c> after the entity configurations, so it sees all of them.</summary>
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
            }
        }
    }
}
