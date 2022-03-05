namespace Logistics.Blazor.Pagination;

public partial class Pager : ComponentBase
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int PagesCount { get; set; } = 2;

    [Parameter]
    public EventCallback<PageEventArgs> OnPageChanged { get; set; }

    private async Task PagerButtonClicked(int page)
    {
        CurrentPage = page;
        var args = new PageEventArgs { Page = this.CurrentPage };
        await OnPageChanged.InvokeAsync(args);
    }
}
