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
            i.CreatedDate >= startPeriod && i.CreatedDate <= endPeriod;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Payment, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "paymentdate" => i => i.PaymentDate,
            "method" => i => i.Method,
            "amount" => i => i.Amount,
            "status" => i => i.Status,
            "comment" => i => i.Comment!,
            "paymentfor" => i => i.PaymentFor,
            "billingaddress" => i => i.BillingAddress,
            _ => i => i.CreatedDate
        };
    }
}
