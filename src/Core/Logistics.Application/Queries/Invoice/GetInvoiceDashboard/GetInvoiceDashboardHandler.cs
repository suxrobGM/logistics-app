using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetInvoiceDashboardHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetInvoiceDashboardQuery, Result<InvoiceDashboardDto>>
{
    public async Task<Result<InvoiceDashboardDto>> Handle(GetInvoiceDashboardQuery req, CancellationToken ct)
    {
        var invoiceRepo = tenantUow.Repository<Invoice>();

        // Get all load invoices (not payroll/subscription)
        var invoices = await invoiceRepo.GetListAsync(
            i => i is LoadInvoice,
            ct);

        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Calculate stats
        var draftInvoices = invoices.Where(i => i.Status == InvoiceStatus.Draft).ToList();
        var pendingInvoices = invoices.Where(i => i.Status == InvoiceStatus.Issued && (i.DueDate == null || i.DueDate >= now)).ToList();
        var overdueInvoices = invoices.Where(i =>
            (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid) &&
            i.DueDate.HasValue && i.DueDate < now).ToList();
        var partiallyPaidInvoices = invoices.Where(i => i.Status == InvoiceStatus.PartiallyPaid && (i.DueDate == null || i.DueDate >= now)).ToList();
        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();

        // Calculate amounts for partially paid invoices
        decimal partiallyPaidRemaining = 0;
        foreach (var invoice in partiallyPaidInvoices.Concat(overdueInvoices.Where(i => i.Status == InvoiceStatus.PartiallyPaid)))
        {
            var paid = invoice.Payments.Sum(p => p.Amount.Amount);
            partiallyPaidRemaining += invoice.Total.Amount - paid;
        }

        // Calculate payments this month
        var paymentsThisMonth = invoices
            .SelectMany(i => i.Payments)
            .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= firstOfMonth)
            .Sum(p => p.Amount.Amount);

        // Get recent invoices (last 10)
        var recentInvoices = invoices
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(i => i.ToDto())
            .ToList();

        var dashboard = new InvoiceDashboardDto
        {
            DraftCount = draftInvoices.Count,
            DraftAmount = draftInvoices.Sum(i => i.Total.Amount),
            PendingCount = pendingInvoices.Count,
            PendingAmount = pendingInvoices.Sum(i => i.Total.Amount),
            OverdueCount = overdueInvoices.Count,
            OverdueAmount = overdueInvoices.Sum(i => i.Total.Amount),
            PartiallyPaidCount = partiallyPaidInvoices.Count,
            PartiallyPaidAmount = partiallyPaidRemaining,
            PaidCount = paidInvoices.Count,
            PaidAmount = paidInvoices.Sum(i => i.Total.Amount),
            TotalOutstanding = pendingInvoices.Sum(i => i.Total.Amount) +
                              overdueInvoices.Sum(i => i.Total.Amount) +
                              partiallyPaidRemaining,
            CollectedThisMonth = paymentsThisMonth,
            RecentInvoices = recentInvoices
        };

        return Result<InvoiceDashboardDto>.Ok(dashboard);
    }
}
