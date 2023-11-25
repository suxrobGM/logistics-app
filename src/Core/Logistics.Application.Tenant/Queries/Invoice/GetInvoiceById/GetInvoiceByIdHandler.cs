using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetInvoiceByIdHandler : RequestHandler<GetInvoiceByIdQuery, ResponseResult<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetInvoiceByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<InvoiceDto>> HandleValidated(
        GetInvoiceByIdQuery req, CancellationToken cancellationToken)
    {
        var invoiceEntity = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoiceEntity is null)
        {
            return ResponseResult<InvoiceDto>.CreateError($"Could not find an invoice with ID {req.Id}");
        }

        var invoiceDto = invoiceEntity.ToDto();
        return ResponseResult<InvoiceDto>.CreateSuccess(invoiceDto);
    }
}
