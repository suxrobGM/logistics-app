namespace Logistics.Application.Tenant.Queries;

public abstract class IntervalQuery<T> : Request<ResponseResult<T>>
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}