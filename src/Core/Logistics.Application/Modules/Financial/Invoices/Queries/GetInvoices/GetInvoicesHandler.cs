using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class GetInvoicesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    public Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery req, CancellationToken ct)
    {
        var filtersLoadFields = req.LoadId.HasValue
                                || req.CustomerId.HasValue
                                || req.FactoringStatus.HasValue;
        if (req.Type == InvoiceType.Load || filtersLoadFields)
        {
            return FilterLoadInvoices(req).ToPagedResultAsync(req, i => i.ToDto(), ct);
        }

        if (req.Type == InvoiceType.Payroll || req.EmployeeId.HasValue)
        {
            return FilterPayrollInvoices(req).ToPagedResultAsync(req, i => i.ToDto(), ct);
        }

        var query = tenantUow.Repository<Invoice>().Query();
        if (req.Type is not null)
        {
            query = query.Where(i => i.Type == req.Type);
        }

        return query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }

    private IQueryable<LoadInvoice> FilterLoadInvoices(GetInvoicesQuery req)
    {
        var query = tenantUow.Repository<LoadInvoice>().Query();

        if (req.LoadId.HasValue)
        {
            query = query.Where(i => i.LoadId == req.LoadId.Value);
        }

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (req.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == req.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();

            // A number matches the invoice or load number as well as the customer name.
            query = long.TryParse(req.Search, out var number)
                ? query.Where(i => i.Number == number
                                   || i.Load.Number == number
                                   || i.Customer.Name.ToLower().Contains(search))
                : query.Where(i => i.Customer.Name.ToLower().Contains(search));
        }

        if (req.FromDate.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= req.FromDate.Value);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(i => i.CreatedAt <= req.ToDate.Value);
        }

        if (req.OverdueOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(i => i.DueDate.HasValue
                                     && i.DueDate.Value < DateOnly.FromDateTime(now)
                                     && i.Status != InvoiceStatus.Paid
                                     && i.Status != InvoiceStatus.Cancelled);
        }

        if (req.FactoringStatus.HasValue)
        {
            query = query.Where(i => i.FactoringStatus == req.FactoringStatus.Value);
        }

        return query;
    }

    private IQueryable<PayrollInvoice> FilterPayrollInvoices(GetInvoicesQuery req)
    {
        var query = tenantUow.Repository<PayrollInvoice>().Query();

        if (req.EmployeeId.HasValue)
        {
            query = query.Where(i => i.EmployeeId == req.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            query = long.TryParse(req.Search, out var number)
                ? query.Where(i => i.Number == number)
                : query.Where(i => i.Employee.FirstName.ToLower().Contains(search)
                                   || i.Employee.LastName.ToLower().Contains(search));
        }

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (req.SalaryType.HasValue)
        {
            query = query.Where(i => i.Employee.SalaryType == req.SalaryType.Value);
        }

        // A payroll matches when its period overlaps the requested range.
        if (req.FromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(req.FromDate.Value);
            query = query.Where(i => i.PeriodEnd >= from);
        }

        if (req.ToDate.HasValue)
        {
            var to = DateOnly.FromDateTime(req.ToDate.Value);
            query = query.Where(i => i.PeriodStart <= to);
        }

        return query;
    }
}
