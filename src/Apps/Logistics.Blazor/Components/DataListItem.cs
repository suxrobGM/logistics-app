namespace Logistics.Blazor;

public class DataListItem
{
    public DataListItem(string value)
    {
        Value = value;
        DisplayName = value;
    }

    public DataListItem(string value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public string Value { get; }
    public string DisplayName { get; }
}
