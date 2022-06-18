namespace Logistics.Application.Handlers.Queries;

internal sealed class GetUsersHandler : RequestHandlerBase<GetEmployeesQuery, PagedDataResult<EmployeeDto>>
{
    private readonly ITenantRepository<Employee> _userRepository;

    public GetUsersHandler(ITenantRepository<Employee> userRepository)
    {
        _userRepository = userRepository;
    }

    protected override Task<PagedDataResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery request, 
        CancellationToken cancellationToken)
    {
        var totalItems = _userRepository.GetQuery().Count();
        var itemsQuery = _userRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _userRepository.GetQuery(new EmployeeSpecification(request.Search));
        }

        var items = itemsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new EmployeeDto
            {
                Id = i.Id,
                ExternalId = i.ExternalId!,
                UserName = i.UserName!,
                FirstName = i.FirstName,
                LastName = i.LastName,
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<EmployeeDto>(items, totalItems, totalPages));
    }

    protected override bool Validate(GetEmployeesQuery request, out string errorDescription)
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

        return string.IsNullOrEmpty(errorDescription);
    }
}
