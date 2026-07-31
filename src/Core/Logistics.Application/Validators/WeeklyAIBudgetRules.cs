using FluentValidation;

namespace Logistics.Application.Validators;

/// <summary>
/// The weekly AI budget rule, shared by the two subscription-plan commands and the AI settings
/// screen. One owner so the three cannot drift on what an empty or zero budget means.
/// </summary>
public static class WeeklyAIBudgetRules
{
    public const string Message =
        "Weekly AI budget must be greater than zero; leave it empty for unlimited.";

    /// <summary>
    /// Null is unlimited; zero or negative would block every session while still looking configured.
    /// </summary>
    public static bool IsValid(decimal? budget) => budget is null or > 0;

    public static IRuleBuilderOptions<T, decimal?> ValidWeeklyAIBudget<T>(
        this IRuleBuilder<T, decimal?> rule) =>
        rule.Must(IsValid).WithMessage(Message);
}
