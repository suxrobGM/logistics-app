using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetInvoiceByIdHandler : RequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetInvoiceByIdHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result<InvoiceDto>> Handle(
        GetInvoiceByIdQuery req, CancellationToken ct)
    {
        var invoiceEntity = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id, ct);

        if (invoiceEntity is null)
        {
            return Result<InvoiceDto>.Fail($"Could not find an invoice with ID {req.Id}");
        }

        var invoiceDto = invoiceEntity.ToDto();
        return Result<InvoiceDto>.Succeed(invoiceDto);
    }
}
