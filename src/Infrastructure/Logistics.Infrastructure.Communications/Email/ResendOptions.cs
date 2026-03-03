namespace Logistics.Infrastructure.Communications.Email;

public record ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public string SenderName { get; set; } = "LogisticsX";
}
