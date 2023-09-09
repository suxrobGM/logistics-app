namespace Logistics.BlazorComponents;

public partial class ProgressBar : ComponentBase
{
    [Parameter]
    public int MaxValue { get; set; } = 100;

    [Parameter]
    public int MinValue { get; set; }

    [Parameter]
    public int Value { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    public int GetPercentage()
    {
        if (Value < MinValue)
            return 0;

        if (MaxValue < 1)
            return 0;

        return Value * 100 / MaxValue;
    }
}
