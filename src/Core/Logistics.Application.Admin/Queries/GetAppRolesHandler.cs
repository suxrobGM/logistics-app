using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetAppRolesHandler : RequestHandlerBase<GetAppRolesQuery, PagedResponseResult<AppRoleDto>>
{
    private readonly IMainRepository _repository;

    public GetAppRolesHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<AppRoleDto>> HandleValidated(
        GetAppRolesQuery query, CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<AppRole>().Count();

        var rolesDto = _repository
            .ApplySpecification(new SearchAppRoles(query.Search))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new AppRoleDto()
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);
        return Task.FromResult(new PagedResponseResult<AppRoleDto>(rolesDto, totalItems, totalPages));
    }
}