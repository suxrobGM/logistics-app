using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetSubscriptions : BaseSpecification<Subscription>
{
    public GetSubscriptions(
        string? orderBy,
        int page,
        int pageSize)
    {
        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    // protected override Expression<Func<Subscription, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "status" => i => i.Status,
    //         "tenantname" => i => i.Tenant.Name,
    //         "tenantcompanyname" => i => i.Tenant.CompanyName,
    //         "planname" => i => i.Plan.Name,
    //         "nextbillingdate" => i => i.NextBillingDate,
    //         "startdate" => i => i.StartDate,
    //         "enddate" => i => i.EndDate,
    //         "trialenddate" => i => i.TrialEndDate,
    //         _ => i => i.StartDate
    //     };
    // }
}
