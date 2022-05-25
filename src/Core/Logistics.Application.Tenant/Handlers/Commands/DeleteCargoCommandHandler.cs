namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteCargoCommandHandler : RequestHandlerBase<DeleteCargoCommand, DataResult>
{
    private readonly ITenantRepository<Cargo> _cargoRepository;

    public DeleteCargoCommandHandler(ITenantRepository<Cargo> cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteCargoCommand request, CancellationToken cancellationToken)
    {
        _cargoRepository.Delete(request.Id!);
        await _cargoRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(DeleteCargoCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
