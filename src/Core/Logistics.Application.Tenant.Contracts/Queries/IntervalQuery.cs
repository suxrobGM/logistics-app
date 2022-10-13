namespace Logistics.Application.Tenant.Queries;

public abstract class IntervalQuery<T> : RequestBase<ResponseResult<T>>
{
    protected IntervalQuery()
    {
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow;
    }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}