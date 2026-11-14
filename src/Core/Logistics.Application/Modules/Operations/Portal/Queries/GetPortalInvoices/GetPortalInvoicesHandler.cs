using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Portal.Queries;

internal sealed class GetPortalInvoicesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPortalInvoicesQuery, PagedResult<PortalInvoiceDto>>
{
    private static readonly SortFields<LoadInvoice> Sorts = new SortFields<LoadInvoice>("-createdAt")
        .Add("number", i => i.Number)
        .Add("status", i => i.Status)
        .Add("total", i => i.Total)
        .Add("createdAt", i => i.CreatedAt);

    public async Task<PagedResult<PortalInvoiceDto>> Handle(
        GetPortalInvoicesQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<LoadInvoice>().Query()
            .Where(i => i.CustomerId == req.CustomerId);

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(i =>
                i.Number.ToString().Contains(search) ||
                (i.Load != null && i.Load.Name.ToLower().Contains(search)));
        }

        if (req.FromDate is { } fromDate)
        {
            baseQuery = baseQuery.Where(i => i.CreatedAt >= fromDate);
        }

        if (req.ToDate is { } toDate)
        {
            baseQuery = baseQuery.Where(i => i.CreatedAt <= toDate);
        }

        var totalItems = await baseQuery.CountAsync(ct);

        var invoices = await Sorts.Apply(baseQuery, req.OrderBy, i => i.Id)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => new PortalInvoiceDto
            {
                Id = i.Id,
                Number = i.Number,
                Status = i.Status,
                Total = i.Total,
                LoadId = i.LoadId,
                LoadNumber = i.Load != null ? i.Load.Number : null,
                LoadName = i.Load != null ? i.Load.Name : null,
                CreatedAt = i.CreatedAt,
                PaidAt = i.Payments.Any(p => p.Status == PaymentStatus.Paid)
                    ? i.Payments.Where(p => p.Status == PaymentStatus.Paid).Max(p => p.CreatedAt)
                    : null,
                DueDate = i.DueDate,
                RemitTo = i.Customer.FactoringCompany == null
                    ? null
                    : new RemitToDto { Name = i.Customer.FactoringCompany.Name, Address = i.Customer.FactoringCompany.RemitToAddress }
            }).ToArrayAsync(ct);

        return PagedResult<PortalInvoiceDto>.Ok(invoices, totalItems, req.PageSize);
    }
}
