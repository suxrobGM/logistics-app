using System.Reflection;
using Microsoft.AspNetCore.Components.Rendering;

namespace Logistics.Blazor.Grids;

public partial class Column : ComponentBase
{
    private RenderFragment? headerCellTemplate;
    private RenderFragment<object>? bodyCellTemplate;

    [Parameter]
    public string? HeaderText { get; set; }

    [Parameter]
    public string? Format { get; set; }

    [Parameter]
    public int? Width { get; set; }

    [Parameter]
    public string? Field { get; set; }

    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<object>? Template { get; set; }

    [Parameter]
    public TextAlign? HeaderTextAlign { get; set; }

    [Parameter]
    public TextAlign TextAlign { get; set; }

    [CascadingParameter]
    private object? OwnerGrid { get; set; }

    public SortDirection? SortDirection { get; set; }

    internal RenderFragment HeaderCellTemplate => headerCellTemplate ??= RenderHeader;

    internal RenderFragment<object> BodyCellTemplate => bodyCellTemplate ??= RenderCell;

    protected override void OnInitialized()
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        OwnerGrid?.GetType().GetMethod("AddColumn", flags)?.Invoke(OwnerGrid, new[] { this });
    }

    private void RenderHeader(RenderTreeBuilder builder)
    {
        if (HeaderTemplate != null)
        {
            builder.AddContent(0, HeaderTemplate);
            return;
        }

        var headerText = HeaderText ?? Field;
        builder.AddContent(0, headerText);
    }

    private RenderFragment RenderCell(object item)
    {
        return builder =>
        {
            if (!string.IsNullOrEmpty(Field))
            {
                var value = item.GetType().GetProperty(Field)?.GetValue(item);
                var formattedValue = string.IsNullOrEmpty(Format) ? value?.ToString() : string.Format("{0:" + Format + "}", value);
                builder.AddContent(0, formattedValue);
            }
            else
            {
                builder.AddContent(1, Template, item);
            }
        };
    }
}
