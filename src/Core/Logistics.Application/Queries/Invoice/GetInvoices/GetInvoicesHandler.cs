using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetInvoicesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    public Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery req, CancellationToken ct)
    {
        // Determine which type of invoices to query
        if (req.InvoiceType == InvoiceType.Load || req.LoadId.HasValue ||
            req.CustomerId.HasValue || !string.IsNullOrEmpty(req.CustomerName))
        {
            return GetLoadInvoices(req);
        }

        if (req.EmployeeId.HasValue || !string.IsNullOrEmpty(req.EmployeeName) ||
            req.InvoiceType == InvoiceType.Payroll)
        {
            return GetPayrollInvoices(req);
        }

        return GetAllInvoices(req);
    }

    private async Task<PagedResult<InvoiceDto>> GetAllInvoices(GetInvoicesQuery req)
    {
        var totalItems = await tenantUow.Repository<Invoice>().CountAsync();
        var invoicesDto = tenantUow.Repository<Invoice>()
            .ApplySpecification(new GetInvoices(req.InvoiceType, req.OrderBy, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, req.PageSize);
    }

    private async Task<PagedResult<InvoiceDto>> GetLoadInvoices(GetInvoicesQuery req)
    {
        var specification = new FilterLoadInvoices(
            req.LoadId,
            req.Status,
            req.CustomerId,
            req.CustomerName,
            req.Search,
            req.StartDate,
            req.EndDate,
            req.OverdueOnly,
            req.OrderBy,
            req.Page,
            req.PageSize);

        var repository = tenantUow.Repository<LoadInvoice>();

        // Count with the same filter criteria (excluding pagination)
        var countSpec = new FilterLoadInvoices(
            req.LoadId,
            req.Status,
            req.CustomerId,
            req.CustomerName,
            req.Search,
            req.StartDate,
            req.EndDate,
            req.OverdueOnly,
            null,
            1,
            int.MaxValue);

        var totalItems = repository.ApplySpecification(countSpec).Count();

        var invoicesDto = repository
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, req.PageSize);
    }

    private Task<PagedResult<InvoiceDto>> GetPayrollInvoices(GetInvoicesQuery req)
    {
        var specification = new FilterPayrollInvoices(
            req.EmployeeId,
            req.EmployeeName,
            req.Status,
            req.SalaryType,
            req.StartDate,
            req.EndDate,
            req.OrderBy,
            req.Page,
            req.PageSize);

        var repository = tenantUow.Repository<PayrollInvoice>();

        // Count with the same filter criteria (excluding pagination)
        var countSpec = new FilterPayrollInvoices(
            req.EmployeeId,
            req.EmployeeName,
            req.Status,
            req.SalaryType,
            req.StartDate,
            req.EndDate,
            null,
            1,
            int.MaxValue);

        var totalItems = repository.ApplySpecification(countSpec).Count();

        var invoicesDto = repository
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<InvoiceDto>.Succeed(invoicesDto, totalItems, req.PageSize));
    }
}
