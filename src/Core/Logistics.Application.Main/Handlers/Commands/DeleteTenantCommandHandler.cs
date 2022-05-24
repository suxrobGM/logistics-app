namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTenantCommandHandler : RequestHandlerBase<DeleteTenantCommand, DataResult>
{
    protected override Task<DataResult> HandleValidated(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(DeleteTenantCommand request, out string errorDescription)
    {
        throw new NotImplementedException();
    }
}
