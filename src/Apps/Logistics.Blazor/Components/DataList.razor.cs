namespace Logistics.Blazor;

public partial class DataList : ComponentBase
{
    private IEnumerable<DataListItem> _items = new List<DataListItem>();

    [Parameter]
    public Func<string, Task<IEnumerable<DataListItem>>>? OnInputChanged { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetId()
    {
        return this.GetHashCode().ToString();
    }

    private async void OnInputChangedHandler(ChangeEventArgs e)
    {
        if (e?.Value == null || OnInputChanged == null)
        {
            return;
        }

        await Task.Delay(500);
        var items = await OnInputChanged.Invoke((string)e.Value);

        if (items != null)
        {
            _items = items.Take(10);
            StateHasChanged();
        }
    }

    private async void OnClickOption(string? displayName)
    {
        var dataValue = _items.FirstOrDefault(i => i.DisplayName == displayName)?.Value;
        await ValueChanged.InvokeAsync(dataValue);
    }
}
