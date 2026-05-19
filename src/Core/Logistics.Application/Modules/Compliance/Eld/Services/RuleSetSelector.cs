using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

/// <summary>
/// Resolves the regulatory rule set for a tenant's region. Used for display
/// (HOS limits endpoint) and historical stamping on incoming violations.
/// HOS data itself is always provider-sourced; this helper does not evaluate
/// or override violation values.
/// </summary>
public static class RuleSetSelector
{
    public static string CodeFor(Region region) => region switch
    {
        Region.EU => HosLimits.Eu561Code,
        _ => HosLimits.FmcsaCode
    };

    public static HosLimits LimitsFor(Region region) => region switch
    {
        Region.EU => HosLimits.Eu561_2006(),
        _ => HosLimits.Fmcsa()
    };
}
