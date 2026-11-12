namespace Logistics.Infrastructure.Integrations.LoadBoard.Credit;

/// <summary>
/// FMCSA QCMobile API options. A free webKey can be requested at
/// https://mobile.fmcsa.dot.gov/QCDevsite/. When no key is configured the FMCSA
/// authority lookup is skipped.
/// </summary>
public class FmcsaOptions
{
    public const string SectionName = "Fmcsa";

    public string BaseUrl { get; set; } = "https://mobile.fmcsa.dot.gov/qc/services/";

    public string? WebKey { get; set; }
}
