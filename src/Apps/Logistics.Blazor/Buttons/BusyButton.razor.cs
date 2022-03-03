namespace Logistics.Blazor.Buttons;

public partial class BusyButton : ComponentBase
{
    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string? BusyText { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public double SpinnerSize { get; set; } = 1.5;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<bool> IsBusyChanged { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }
}
