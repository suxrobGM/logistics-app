using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetInvoiceByIdHandler : RequestHandler<GetInvoiceByIdQuery, ResponseResult<InvoiceDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetInvoiceByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<InvoiceDto>> HandleValidated(
        GetInvoiceByIdQuery req, CancellationToken cancellationToken)
    {
        var invoiceEntity = await _tenantRepository.GetAsync<Invoice>(req.Id);
        
        if (invoiceEntity is null)
            return ResponseResult<InvoiceDto>.CreateError($"Could not find an invoice with ID {req.Id}");

        var invoiceDto = invoiceEntity.ToDto();
        return ResponseResult<InvoiceDto>.CreateSuccess(invoiceDto);
    }
}
