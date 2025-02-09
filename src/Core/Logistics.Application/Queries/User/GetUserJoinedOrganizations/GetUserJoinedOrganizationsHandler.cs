using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetUserJoinedOrganizationsHandler : 
    RequestHandler<GetUserJoinedOrganizationsQuery, Result<OrganizationDto[]>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetUserJoinedOrganizationsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<OrganizationDto[]>> HandleValidated(
        GetUserJoinedOrganizationsQuery req, 
        CancellationToken cancellationToken)
    {
        var user = await _masterUow.Repository<User>().GetByIdAsync(req.UserId);

        if (user is null)
        {
            return Result<OrganizationDto[]>.Fail($"Could not find an user with ID '{req.UserId}'");
        }

        var organizations = await _masterUow.Repository<Tenant>().GetListAsync(i => i.Id == user.TenantId);

        var organizationsDto = organizations
            .Select(i => new OrganizationDto
            {
                TenantId = i.Id,
                Name = i.Name,
                DisplayName = i.CompanyName!
            })
            .ToArray();
        
        return Result<OrganizationDto[]>.Succeed(organizationsDto);
    }
}
