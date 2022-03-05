namespace Logistics.Blazor.Grids;

public class PageSettings
{
    public int PagesCount { get; set; } = 2;
    public EventCallback<PageEventArgs> OnPageChanged { get; set; }
}
