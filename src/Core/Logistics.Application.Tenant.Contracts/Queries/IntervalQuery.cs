namespace Logistics.Application.Contracts.Queries;

public abstract class IntervalQuery<T> : RequestBase<DataResult<T>>
{
    protected IntervalQuery()
    {
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow;
    }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}