namespace Logistics.Application.Admin.Queries;

internal sealed class GetUserJoinedOrganizationsHandler :
    RequestHandlerBase<GetUserJoinedOrganizationsQuery, ResponseResult<UserOrganizationsDto>>
{
    private readonly IMainRepository _repository;

    public GetUserJoinedOrganizationsHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult<UserOrganizationsDto>> HandleValidated(
        GetUserJoinedOrganizationsQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetAsync<User>(request.UserId);

        if (user == null)
            return ResponseResult<UserOrganizationsDto>.CreateError("Could not find the specified user");
        
        var userOrganizationsDto = new UserOrganizationsDto() { Organizations = user.GetJoinedTenants() };
        return ResponseResult<UserOrganizationsDto>.CreateSuccess(userOrganizationsDto);
    }
}
