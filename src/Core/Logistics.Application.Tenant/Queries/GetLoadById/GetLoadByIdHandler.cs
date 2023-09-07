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
        GetLoadByIdQuery req, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantRepository.GetAsync<Load>(req.Id);

        if (loadEntity == null)
            return ResponseResult<LoadDto>.CreateError("Could not find the specified cargo");

        var assignedDispatcher = await _mainRepository.GetAsync<User>(loadEntity.AssignedDispatcherId);

        var loadDto = LoadMapper.ToDto(loadEntity)!;
        loadDto.AssignedDispatcherName = assignedDispatcher?.GetFullName();
        loadDto.AssignedTruckNumber = loadEntity.AssignedTruck?.TruckNumber;
        loadDto.AssignedTruckId = loadDto.AssignedTruckId;
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
