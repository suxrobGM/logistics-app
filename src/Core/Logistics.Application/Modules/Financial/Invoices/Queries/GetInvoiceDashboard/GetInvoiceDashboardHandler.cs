using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class GetInvoiceDashboardHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetInvoiceDashboardQuery, Result<InvoiceDashboardDto>>
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
        var openInvoices = invoices.Where(i => InvoiceStatuses.Open.Contains(i.Status)).ToList();
        var overdueInvoices = openInvoices.Where(i => i.DueDate < now).ToList();
        var notDueInvoices = openInvoices.Except(overdueInvoices).ToList();
        var pendingInvoices = notDueInvoices.Where(i => i.Status != InvoiceStatus.PartiallyPaid).ToList();
        var partiallyPaidInvoices = notDueInvoices.Where(i => i.Status == InvoiceStatus.PartiallyPaid).ToList();
        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();

        var overdueRemaining = overdueInvoices.Sum(Outstanding);
        var partiallyPaidRemaining = partiallyPaidInvoices.Sum(Outstanding);

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

        // A funded invoice is settled by the factor's purchase; what the factor still owes is its reserve.
        var openLoadInvoices = openInvoices.OfType<LoadInvoice>().ToList();
        var fundedItems = await tenantUow.Repository<FactoringBatchItem>()
            .GetListAsync(i => i.Status == FactoringStatus.Funded, ct);

        var dashboard = new InvoiceDashboardDto
        {
            DraftCount = draftInvoices.Count,
            DraftAmount = draftInvoices.Sum(i => i.Total.Amount),
            PendingCount = pendingInvoices.Count,
            PendingAmount = pendingInvoices.Sum(i => i.Total.Amount),
            OverdueCount = overdueInvoices.Count,
            OverdueAmount = overdueRemaining,
            PartiallyPaidCount = partiallyPaidInvoices.Count,
            PartiallyPaidAmount = partiallyPaidRemaining,
            PaidCount = paidInvoices.Count,
            PaidAmount = paidInvoices.Sum(i => i.Total.Amount),
            TotalOutstanding = pendingInvoices.Sum(i => i.Total.Amount) + overdueRemaining + partiallyPaidRemaining,
            CollectedThisMonth = paymentsThisMonth,
            OwedByCustomers = openLoadInvoices.Where(i => i.FactoringStatus != FactoringStatus.Submitted).Sum(Outstanding),
            OwedByFactor = openLoadInvoices.Where(i => i.FactoringStatus == FactoringStatus.Submitted).Sum(Outstanding)
                           + fundedItems.Sum(i => i.ReserveOutstanding),
            RecentInvoices = recentInvoices
        };

        return Result<InvoiceDashboardDto>.Ok(dashboard);
    }

    private static decimal Outstanding(Invoice invoice) =>
        invoice.Total.Amount - invoice.Payments.Sum(p => p.Amount.Amount);
}
