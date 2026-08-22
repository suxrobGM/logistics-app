namespace Logistics.Application.Modules.Integrations.Negotiation;

/// <summary>
/// The RFC 5322 Message-ID an outbound counter-offer is sent with. Generated here rather than read
/// back from the provider, whose id is an internal handle no mail server ever sees: the same value
/// goes on the outgoing header, onto the stored message row, and into the next round's
/// In-Reply-To / References, so all three must agree or the broker's client starts a new thread.
/// </summary>
public static class NegotiationMessageId
{
    /// <summary>
    /// <c>&lt;neg-{unique}@{replyDomain}&gt;</c>. The angle brackets belong to the value - a msg-id
    /// carries them in every header it appears in, and receiving clients match on the literal text.
    /// </summary>
    public static string Create(string replyDomain) =>
        $"<neg-{Guid.NewGuid():N}@{replyDomain}>";
}
