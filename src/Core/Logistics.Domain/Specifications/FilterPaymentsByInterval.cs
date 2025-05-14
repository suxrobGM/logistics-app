using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class FilterPaymentsByInterval : BaseSpecification<Payment>
{
    public FilterPaymentsByInterval(
        string? orderBy,
        DateTime? startPeriod,
        DateTime endPeriod,
        int page,
        int pageSize)
    {
        Criteria = i =>
            i.CreatedAt >= startPeriod && i.CreatedAt <= endPeriod;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Payment, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "amount" => i => i.Amount,
            "status" => i => i.Status,
            "billingaddress" => i => i.BillingAddress,
            _ => i => i.CreatedAt
        };
    }
}
