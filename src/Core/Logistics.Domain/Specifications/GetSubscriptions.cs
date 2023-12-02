using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetSubscriptions : BaseSpecification<Subscription>
{
    public GetSubscriptions(
        string? orderProperty,
        int page,
        int pageSize,
        bool descending)
    {
        ApplyOrderBy(InitOrderBy(orderProperty), descending);
        ApplyPaging(page, pageSize);
    }
    
    private static Expression<Func<Subscription, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower();
        return propertyName switch
        {
            "status" => i => i.Status,
            "tenantname" => i => i.Tenant.Name,
            "tenantcompanyname" => i => i.Tenant.CompanyName,
            "planname" => i => i.Plan.Name,
            "nextpaymentdate" => i => i.NextPaymentDate,
            "startdate" => i => i.StartDate,
            "enddate" => i => i.EndDate,
            "trialenddate" => i => i.TrialEndDate,
            _ => i => i.StartDate
        };
    }
}
