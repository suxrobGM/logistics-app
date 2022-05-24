namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateTenantCommandHandler : RequestHandlerBase<UpdateTenantCommand, DataResult>
{
    protected override Task<DataResult> HandleValidated(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(UpdateTenantCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

