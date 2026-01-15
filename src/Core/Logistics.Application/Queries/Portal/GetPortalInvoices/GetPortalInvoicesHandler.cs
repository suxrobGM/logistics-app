using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetPortalInvoicesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPortalInvoicesQuery, PagedResult<PortalInvoiceDto>>
{
    public async Task<PagedResult<PortalInvoiceDto>> Handle(
        GetPortalInvoicesQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<LoadInvoice>().Query()
            .Include(i => i.Load)
            .Where(i => i.CustomerId == req.CustomerId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(i =>
                i.Number.ToString().Contains(search) ||
                (i.Load != null && i.Load.Name.ToLower().Contains(search)));
        }

        // Apply date filters
        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(i => i.CreatedAt >= req.StartDate && i.CreatedAt <= req.EndDate);
        }

        var totalItems = await baseQuery.CountAsync(ct);

        // Apply ordering
        baseQuery = req.OrderBy?.ToLower() switch
        {
            "number" => baseQuery.OrderBy(i => i.Number),
            "number_desc" => baseQuery.OrderByDescending(i => i.Number),
            "status" => baseQuery.OrderBy(i => i.Status),
            "total" => baseQuery.OrderBy(i => i.Total.Amount),
            "total_desc" => baseQuery.OrderByDescending(i => i.Total.Amount),
            "created" => baseQuery.OrderBy(i => i.CreatedAt),
            _ => baseQuery.OrderByDescending(i => i.CreatedAt)
        };

        // Apply paging
        baseQuery = baseQuery.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);

        var invoices = await baseQuery.Select(i => new PortalInvoiceDto
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
            DueDate = i.DueDate
        }).ToArrayAsync(ct);

        return PagedResult<PortalInvoiceDto>.Succeed(invoices, totalItems, req.PageSize);
    }
}
