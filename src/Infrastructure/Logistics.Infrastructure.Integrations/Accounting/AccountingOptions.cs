namespace Logistics.Infrastructure.Integrations.Accounting;

public record AccountingOptions
{
    public const string SectionName = "Accounting";
    public QuickBooksOptions? QuickBooks { get; set; }
}

public record QuickBooksOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    /// <summary>"sandbox" or "production" - selects the QBO API base URLs.</summary>
    public string Environment { get; set; } = "sandbox";

    /// <summary>OAuth2 redirect URI registered with Intuit; must match the callback endpoint.</summary>
    public string? RedirectUri { get; set; }

    /// <summary>
    /// TMS portal URL to redirect the browser back to after the OAuth callback completes.
    /// A "status=success|error" query parameter is appended.
    /// </summary>
    public string? PortalReturnUrl { get; set; }

    /// <summary>OAuth2 scopes requested. Defaults to the accounting scope.</summary>
    public string Scopes { get; set; } = "com.intuit.quickbooks.accounting";
}
