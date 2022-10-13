namespace Logistics.Application.Main.Handlers.Queries;

internal sealed class GetAppRolesHandler : RequestHandlerBase<GetAppRolesRequest, PagedResponseResult<AppRoleDto>>
{
    private readonly IMainRepository _repository;

    public GetAppRolesHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<AppRoleDto>> HandleValidated(
        GetAppRolesRequest request, CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<AppRole>().Count();

        var rolesDto = _repository
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
        return Task.FromResult(new PagedResponseResult<AppRoleDto>(rolesDto, totalItems, totalPages));
    }

    protected override bool Validate(GetAppRolesRequest request, out string errorDescription)
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