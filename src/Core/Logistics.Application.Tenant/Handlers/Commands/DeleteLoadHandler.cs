namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteLoadHandler : RequestHandlerBase<DeleteLoadCommand, DataResult>
{
    private readonly ITenantRepository<Load> _cargoRepository;

    public DeleteLoadHandler(ITenantRepository<Load> cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteLoadCommand request, CancellationToken cancellationToken)
    {
        _cargoRepository.Delete(request.Id!);
        await _cargoRepository.UnitOfWork.CommitAsync();
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
