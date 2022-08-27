namespace Logistics.Application.Handlers.Queries;

public class GetAppRolesHandler : RequestHandlerBase<GetAppRolesQuery, PagedDataResult<AppRoleDto>>
{
    private readonly IMainRepository<AppRole> _rolesRepository;

    public GetAppRolesHandler(IMainRepository<AppRole> rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    protected override Task<PagedDataResult<AppRoleDto>> HandleValidated(GetAppRolesQuery request, CancellationToken cancellationToken)
    {
        var totalItems = _rolesRepository.GetQuery().Count();

        var rolesDto = _rolesRepository
            .ApplySpecification(new SearchAppRoles(request.Search))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new AppRoleDto()
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<AppRoleDto>(rolesDto, totalItems, totalPages));
    }

    protected override bool Validate(GetAppRolesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (request.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }
        return true;
    }
}