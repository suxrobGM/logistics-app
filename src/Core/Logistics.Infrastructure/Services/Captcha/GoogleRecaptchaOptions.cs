namespace Logistics.Infrastructure.Services;

internal class GoogleRecaptchaOptions
{
    public const string SectionName = "GoogleRecaptcha";
    public string? SiteKey { get; set; }
    public string? SecretKey { get; set; }
}
