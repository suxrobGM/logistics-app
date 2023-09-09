namespace Logistics.BlazorComponents;

public partial class AutoComplete : ComponentBase
{
    private IEnumerable<DropdownItem> _dataItems = new List<DropdownItem>();
    private DropdownItem? _selectedItem;
    private string? _currentInputValue;
    //private string _showDropdownClass = "hide";

    [Parameter, EditorRequired]
    public Func<string, Task<IEnumerable<DropdownItem>>>? OnInputChanged { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter] 
    public string Placeholder { get; set; } = string.Empty;

    [Parameter] 
    public int MaximumItems { get; set; } = 10;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetId() => GetHashCode().ToString();

    private async void OnInputChangedHandler(ChangeEventArgs e)
    {
        var inputValue = e.Value as string;
        if (inputValue == null || OnInputChanged == null)
        {
            //_showDropdownClass = "hide";
            StateHasChanged();
            return;
        }

        await Task.Delay(300);
        var items = await OnInputChanged.Invoke(inputValue);

        _dataItems = items.Take(MaximumItems);
        //_showDropdownClass = _dataItems.Any() && !string.IsNullOrEmpty(inputValue) ? "show" : "hide";
        StateHasChanged();
    }

    private void OnInputFocusOutHandler()
    {
        if (_selectedItem == null) 
            return;
        
        _currentInputValue = _selectedItem?.Label;
        //_showDropdownClass = "hide";
        StateHasChanged();
    }

    private async void OnClickDropdownItem(string? value)
    {
        var dropdownItem = _dataItems.FirstOrDefault(i => i.Value == value);
        //_showDropdownClass = "hide";
        _selectedItem = dropdownItem;
        _currentInputValue = dropdownItem?.Label;
        await ValueChanged.InvokeAsync(dropdownItem?.Value);
    }

    private void OnInputChangeHandler(ChangeEventArgs e)
    {
        _currentInputValue = e.Value?.ToString();
    }
}
