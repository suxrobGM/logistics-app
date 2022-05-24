namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTenantCommandHandler : RequestHandlerBase<CreateTenantCommand, DataResult>
{
    protected override Task<DataResult> HandleValidated(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(CreateTenantCommand request, out string errorDescription)
    {
        throw new NotImplementedException();
    }
}
