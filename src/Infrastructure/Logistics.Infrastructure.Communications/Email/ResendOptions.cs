namespace Logistics.Infrastructure.Communications.Email;

public record ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public string SenderName { get; set; } = "LogisticsX";

    /// <summary>Shared secret for verifying inbound Resend webhook signatures.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Domain parsed out of <see cref="SenderEmail"/> - the single place that does this parsing.
    /// Used to build per-thread reply addresses (e.g. <c>offer-{token}@{SenderDomain}</c>).
    /// Handles both a bare address and a "Name &lt;address&gt;" form.
    /// </summary>
    public string SenderDomain
    {
        get
        {
            var address = SenderEmail;
            var angleStart = address.IndexOf('<');
            var angleEnd = address.IndexOf('>');
            if (angleStart >= 0 && angleEnd > angleStart)
            {
                address = address[(angleStart + 1)..angleEnd];
            }

            var atIndex = address.LastIndexOf('@');
            return atIndex >= 0 ? address[(atIndex + 1)..].Trim() : address.Trim();
        }
    }
}
