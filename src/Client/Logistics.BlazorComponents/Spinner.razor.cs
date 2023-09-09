namespace Logistics.BlazorComponents;

public partial class Spinner : ComponentBase
{
    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public bool Overlay { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public Align LabelAlign { get; set; }

    [Parameter]
    public Color LabelColor { get; set; }

    [Parameter]
    public double Size { get; set; } = 3;

    [Parameter]
    public EventCallback<bool> IsBusyChanged { get; set; }

    public enum Align
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum Color
    {
        Inherit,
        White,
        Black
    }

    public async Task ShowAsync()
    {
        IsBusy = true;
        await IsBusyChanged.InvokeAsync(true);
        StateHasChanged();
    }

    public async Task HideAsync()
    {
        IsBusy = false;
        await IsBusyChanged.InvokeAsync(false);
        StateHasChanged();
    }

    private string GetColor(Color color)
    {
        return color switch
        {
            Color.Inherit => "",
            Color.White => "text-white",
            Color.Black => "text-black",
            _ => "",
        };
    }
}
