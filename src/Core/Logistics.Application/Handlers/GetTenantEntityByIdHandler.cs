using Logistics.Application.Abstractions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Handlers;

/// <summary>
///     Base handler for the copy-paste "get a tenant-scoped entity by primary key and map to a DTO"
///     pattern: fetch by ID, fail if missing, map. Subclasses supply the mapping.
/// </summary>
/// <typeparam name="TQuery">Query carrying the target <see cref="IHaveId.Id" />.</typeparam>
/// <typeparam name="TEntity">Tenant-scoped entity to fetch.</typeparam>
/// <typeparam name="TDto">DTO returned on success.</typeparam>
internal abstract class GetTenantEntityByIdHandler<TQuery, TEntity, TDto>(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<TQuery, Result<TDto>>
    where TQuery : IQuery<Result<TDto>>, IHaveId
    where TEntity : class, IEntity<Guid>, ITenantEntity
{
    public async Task<Result<TDto>> Handle(TQuery req, CancellationToken ct)
    {
        var entity = await tenantUow.Repository<TEntity>().GetByIdAsync(req.Id, ct);

        if (entity is null)
        {
            return Result<TDto>.Fail($"Could not find a {EntityName.Humanize<TEntity>()} with ID '{req.Id}'");
        }

        return Result<TDto>.Ok(MapToDto(entity));
    }

    /// <summary>Maps the fetched entity to its DTO.</summary>
    protected abstract TDto MapToDto(TEntity entity);
}
