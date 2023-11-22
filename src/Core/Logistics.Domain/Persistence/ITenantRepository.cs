using Logistics.Domain.Core;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Tenant's repository.
/// </summary>
public interface ITenantRepository<TEntity> : IMasterRepository<TEntity> where TEntity : class, ITenantEntity
{
}
