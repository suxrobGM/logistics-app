using System.Text.Json.Serialization;

namespace Logistics.Application.Tenant.Queries;

public abstract class PagedIntervalQuery<T> : Request<PagedResponseResult<T>>
{
    private string? _orderBy;

    public string? OrderBy
    {
        get => _orderBy;
        set 
        {
            value ??= string.Empty;

            if (value.StartsWith("-"))
            {
                value = value[1..];
                Descending = true;
            }
            _orderBy = value;
        }
    }
    
    [JsonIgnore]
    public bool Descending { get; private set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}
