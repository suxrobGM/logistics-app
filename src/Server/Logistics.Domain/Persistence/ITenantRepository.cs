using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Tenant's repository.
/// </summary>
public interface ITenantRepository<TEntity> : IRepository<TEntity, string> where TEntity : class, ITenantEntity;
