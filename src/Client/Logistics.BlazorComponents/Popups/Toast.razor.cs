namespace Logistics.BlazorComponents.Popups;

public partial class Toast : ComponentBase
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public string? HeaderText { get; set; }

    [Parameter]
    public string? BodyText { get; set; }

    [Parameter]
    public Position ToastPosition { get; set; }

    [Parameter]
    public int Timeout { get; set; } = 10000;

    public enum Position
    {
        CenterOverlay,
        BottomRight
    }

    public void Show(string bodyText, string headerText = "")
    {
        HeaderText = headerText;
        BodyText = bodyText;
        IsVisible = true;
        StateHasChanged();
    }

    private void Close()
    {
        HeaderText = string.Empty;
        BodyText = string.Empty;
        IsVisible = false;
        StateHasChanged();
    }

    private static string GetPosition(Position position)
    {
        return position switch
        {
            Position.CenterOverlay => "toast-center-overlay",
            Position.BottomRight => "toast-bottom-right",
            _ => "toast-center-overlay",
        };
    }
}
