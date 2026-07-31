namespace Logistics.Application.Validators;

/// <summary>
/// The weekly AI budget message, shared by the two subscription-plan commands and the AI settings
/// screen so the three cannot word the same rule differently.
/// </summary>
public static class WeeklyAIBudgetRules
{
    public const string Message = "Weekly AI budget must be greater than zero.";
}
