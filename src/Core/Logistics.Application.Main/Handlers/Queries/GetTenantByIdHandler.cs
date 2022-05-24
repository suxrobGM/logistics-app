namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTenantByIdHandler : RequestHandlerBase<GetTenatByIdQuery, DataResult<TenantDto>>
{
    protected override Task<DataResult<TenantDto>> HandleValidated(GetTenatByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(GetTenatByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
