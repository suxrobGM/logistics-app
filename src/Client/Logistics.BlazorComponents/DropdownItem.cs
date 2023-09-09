namespace Logistics.BlazorComponents;

public class DropdownItem
{
    public DropdownItem(string value)
    {
        Value = value;
        Label = value;
    }

    public DropdownItem(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }
    public string Label { get; }
}
