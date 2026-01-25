using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

/// <summary>
/// Specification for filtering load invoices with comprehensive filter options.
/// </summary>
public sealed class FilterLoadInvoices : BaseSpecification<LoadInvoice>
{
    public FilterLoadInvoices(
        Guid? loadId,
        InvoiceStatus? status,
        Guid? customerId,
        string? customerName,
        string? search,
        DateTime? startDate,
        DateTime? endDate,
        bool? overdueOnly,
        string? orderBy,
        int page,
        int pageSize)
    {
        Expression<Func<LoadInvoice, bool>>? criteria = null;

        // Filter by Load ID
        if (loadId.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.LoadId == loadId.Value);
        }

        // Filter by Status
        if (status.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.Status == status.Value);
        }

        // Filter by Customer ID
        if (customerId.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.CustomerId == customerId.Value);
        }

        // Filter by Customer Name (partial match)
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var lowerName = customerName.ToLower();
            criteria = AddCriteria(criteria, i => i.Customer.Name.ToLower().Contains(lowerName));
        }

        // Search (matches load number, customer name, or invoice number)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();

            // Try to parse as a number for invoice/load number search
            if (long.TryParse(search, out var numberSearch))
            {
                criteria = AddCriteria(criteria, i =>
                    i.Number == numberSearch ||
                    i.Load.Number == numberSearch ||
                    i.Customer.Name.ToLower().Contains(lowerSearch));
            }
            else
            {
                criteria = AddCriteria(criteria, i =>
                    i.Customer.Name.ToLower().Contains(lowerSearch));
            }
        }

        // Filter by date range (based on CreatedAt)
        if (startDate.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            criteria = AddCriteria(criteria, i => i.CreatedAt <= endDate.Value);
        }

        // Filter to show only overdue invoices
        if (overdueOnly == true)
        {
            var now = DateTime.UtcNow;
            criteria = AddCriteria(criteria, i =>
                i.DueDate.HasValue &&
                i.DueDate.Value < now &&
                i.Status != InvoiceStatus.Paid &&
                i.Status != InvoiceStatus.Cancelled);
        }

        if (criteria is not null)
        {
            Criteria = criteria;
        }

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    private static Expression<Func<LoadInvoice, bool>> AddCriteria(
        Expression<Func<LoadInvoice, bool>>? existing,
        Expression<Func<LoadInvoice, bool>> newCriteria)
    {
        if (existing is null)
        {
            return newCriteria;
        }

        // Combine expressions with AND
        var parameter = Expression.Parameter(typeof(LoadInvoice), "i");
        var combined = Expression.AndAlso(
            Expression.Invoke(existing, parameter),
            Expression.Invoke(newCriteria, parameter));

        return Expression.Lambda<Func<LoadInvoice, bool>>(combined, parameter);
    }
}
