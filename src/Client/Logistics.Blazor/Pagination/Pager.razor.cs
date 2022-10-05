namespace Logistics.Blazor.Pagination;

public partial class Pager : ComponentBase
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int PagesCount { get; set; } = 1;

    [Parameter]
    public EventCallback<PageEventArgs> OnPageChanged { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (parameters.TryGetValue<int>("PagesCount", out var pagesCount))
        {
            PagesCount = pagesCount <= 0 ? 1 : pagesCount;
        }
    }

    private async Task PagerButtonClicked(int page)
    {
        CurrentPage = page;
        var args = new PageEventArgs { Page = this.CurrentPage };
        await OnPageChanged.InvokeAsync(args);
    }
}
