namespace Logistics.Application.Modules.Integrations.Negotiation;

/// <summary>
/// The reply address is the only link from inbound broker mail back to a thread, so the sender and
/// the webhook parser must agree on its shape. Both sides go through here rather than repeating the
/// literal, where a changed prefix would break routing with no compile error.
/// </summary>
public static class NegotiationReplyAddress
{
    private const string Prefix = "offer-";

    /// <summary>Stand-in shown in a preview for a thread that does not exist yet.</summary>
    public const string UnassignedToken = "<assigned on send>";

    public static string Format(string replyToken, string replyDomain) =>
        $"{Prefix}{replyToken}@{replyDomain}";

    /// <summary>Reads the thread token out of a delivered-to address, or null if it is not ours.</summary>
    public static string? TryParseToken(string address)
    {
        var localPart = address.Split('@')[0].Trim();

        return localPart.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) &&
               localPart.Length > Prefix.Length
            ? localPart[Prefix.Length..]
            : null;
    }
}
