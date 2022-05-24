namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTenantCommandHandler : RequestHandlerBase<DeleteTenantCommand, DataResult>
{
    protected override Task<DataResult> HandleValidated(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(DeleteTenantCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
