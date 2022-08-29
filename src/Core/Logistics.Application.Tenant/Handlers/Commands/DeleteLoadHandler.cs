namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteLoadHandler : RequestHandlerBase<DeleteLoadCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteLoadHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        DeleteLoadCommand request, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(request.Id);
        _tenantRepository.Delete(load);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
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
