using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadByIdHandler : RequestHandler<GetLoadByIdQuery, ResponseResult<LoadDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetLoadByIdHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<LoadDto>> HandleValidated(
        GetLoadByIdQuery request, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantRepository.GetAsync<Load>(request.Id);

        if (loadEntity == null)
            return ResponseResult<LoadDto>.CreateError("Could not find the specified cargo");

        var assignedDispatcher = await _mainRepository.GetAsync<User>(loadEntity.AssignedDispatcherId);
        var assignedDriver = await _mainRepository.GetAsync<User>(i => loadEntity.AssignedDriver != null && i.Id == loadEntity.AssignedDriver.Id);

        var loadDto = LoadMapper.ToDto(loadEntity)!;
        loadDto.AssignedDispatcherName = assignedDispatcher?.GetFullName();
        loadDto.AssignedDriverName = assignedDriver?.GetFullName();
        return ResponseResult<LoadDto>.CreateSuccess(loadDto);
    }

    protected override bool Validate(GetLoadByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
