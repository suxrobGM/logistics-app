namespace Logistics.Application.Contracts.Filters;

public abstract class BaseFilter
{
    public bool ContainsMode { get; set; } = true;
    public bool IgnoreCaseSensitive { get; set; } = true;
}
