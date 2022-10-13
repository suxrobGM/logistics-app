using System.Text.Json.Serialization;

namespace Logistics.Application.Shared;

public abstract class SearchableRequest<T> : PagedRequest<T>
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
    public string? Search { get; set; }
}
