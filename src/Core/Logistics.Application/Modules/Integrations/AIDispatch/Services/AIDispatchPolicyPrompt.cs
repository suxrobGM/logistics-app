namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// The prompt that turns decision history into a policy. Most of it exists to stop the model
/// inventing rules: every bullet needs repeated evidence, and thin evidence must produce
/// <see cref="NoPolicyToken"/> rather than a plausible guess.
/// </summary>
internal static class AIDispatchPolicyPrompt
{
    /// <summary>Emitted verbatim when the history does not support any rule. Clears the stored policy.</summary>
    public const string NoPolicyToken = "NO_POLICY";

    public const string SystemPrompt = $$"""
        Turn a freight dispatcher's decision history into a short dispatch policy that will be
        injected into an AI dispatch agent's system prompt.

        You will receive one line per decision. REJECTED means the dispatcher refused the agent's
        proposed action and gave a reason. APPROVED means the dispatcher accepted it.

        ## What to produce
        Markdown only. No preamble, no closing prose, no code fences.

        ## Learned preferences
        - <imperative preference> (<N> rejections)

        Optionally, a second section:

        ## Avoid
        - <thing the dispatcher consistently refuses> (<N> rejections)

        ## Hard rules
        1. At most 8 bullets total. At most 400 words.
        2. Every bullet needs at least 3 consistent observations in the data. End each bullet with its
           evidence count, e.g. "(7 rejections)".
        3. Write imperative preferences the agent can act on ("Prefer pickups within 50 miles of the
           truck's current position"), never observations about the data ("Many rejections mention
           deadhead").
        4. NEVER include identifiers, names, dates, load numbers, truck numbers, driver names or any
           other tenant-specific value. Rules must generalise.
        5. NEVER invent a numeric threshold that is not visible in the data. If dispatchers rejected
           loads over "80 miles of deadhead", you may say 80 miles. If no number appears, do not
           produce one.
        6. NEVER restate Hours of Service rules or truck-type compatibility rules. Those are already
           hard constraints in the agent's prompt, and repeating them wastes the budget.
        7. Ignore one-off rejections. A single complaint is noise, not a preference.

        ## When the evidence is thin
        If fewer than 3 decisions support any single rule, output exactly:

        {{NoPolicyToken}}

        Nothing else. An empty policy is correct and expected early on - a fabricated one is not.
        """;

    public static string BuildUserText(string historyDigest, string? existingPolicy)
    {
        var text = $"""
            Decision history (newest first, rejections listed before approvals):

            {historyDigest}
            """;

        if (!string.IsNullOrWhiteSpace(existingPolicy))
        {
            // Anchor on the previous version, or the wording flaps from night to night.
            text += $"""


                The previous version of this policy is below. Keep any rule the history above still
                supports (reuse its wording so the document stays stable), drop rules the history no
                longer supports, and add newly supported rules. Do not simply copy it.

                {existingPolicy}
                """;
        }

        return text;
    }
}
