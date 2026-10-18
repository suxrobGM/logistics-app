namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Durations as the agent should quote them ("12h 45m"). Tools hand over the finished string
/// because the model reformats raw minutes unreliably and the prompts used to spend lines on it.
/// </summary>
internal static class AgentDuration
{
    public static string Format(int minutes)
    {
        var sign = minutes < 0 ? "-" : "";
        var total = Math.Abs(minutes);
        var (hours, rest) = (total / 60, total % 60);

        if (hours == 0)
            return $"{sign}{rest}m";

        if (rest == 0)
            return $"{sign}{hours}h";

        return $"{sign}{hours}h {rest}m";
    }
}
