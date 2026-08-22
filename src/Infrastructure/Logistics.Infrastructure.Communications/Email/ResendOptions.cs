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
    /// Domain parsed out of <see cref="SenderEmail"/>, used to build per-thread reply addresses.
    /// </summary>
    public string SenderDomain => SenderEmail[(SenderEmail.LastIndexOf('@') + 1)..].Trim();
}
