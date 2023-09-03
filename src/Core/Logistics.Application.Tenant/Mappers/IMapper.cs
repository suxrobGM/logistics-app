using Logistics.Domain.Common;

namespace Logistics.Application.Tenant.Mappers;

public interface IMapper<TEntity, TDto> 
    where TEntity : Entity
    where TDto : class 
{
    static abstract TEntity? ToEntity(TDto? dto);
    static abstract TDto? ToDto(TEntity? entity);
}