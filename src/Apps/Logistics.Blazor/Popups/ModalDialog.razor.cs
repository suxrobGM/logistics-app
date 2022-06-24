namespace Logistics.Blazor.Popups;

public partial class ModalDialog : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = "modalDialog";

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public Align DialogAlign { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    public enum Align
    {
        Default,
        VerticallyCenter
    }

    private static string GetAlign(Align align)
    {
        return align switch
        {
            Align.Default => "",
            Align.VerticallyCenter => "modal-dialog-centered",
            _ => "",
        };
    }
}
