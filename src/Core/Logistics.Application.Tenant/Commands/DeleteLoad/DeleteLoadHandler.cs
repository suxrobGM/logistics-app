namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteLoadHandler : RequestHandler<DeleteLoadCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteLoadHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteLoadCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.Id);
        _tenantRepository.Delete(load);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    protected override bool Validate(DeleteLoadCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
