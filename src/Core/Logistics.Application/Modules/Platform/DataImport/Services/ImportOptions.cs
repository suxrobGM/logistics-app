using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

/// <param name="Commit">False for the validation dry run: nothing is written.</param>
/// <param name="UpdateExisting">Rows matching an existing record update it instead of being skipped.</param>
/// <param name="SendInvitations">Drivers only: email each new driver an invitation to set a password.</param>
internal sealed record ImportOptions(
    bool Commit,
    bool UpdateExisting,
    bool SendInvitations,
    Region Region,
    DateFormatType DateFormat,
    string Currency,
    string DefaultCountry)
{
    /// <summary>
    ///     Reads dates, money and countries the way the tenant's own forms do. A blank country falls
    ///     back to the company's own country.
    /// </summary>
    public static ImportOptions For(Tenant tenant, bool commit, bool updateExisting, bool sendInvitations)
    {
        var settings = tenant.Settings;
        var defaultCountry = settings.Region == Region.US
            ? "US"
            : ImportAddressParser.NormalizeCountry(tenant.CompanyAddress.Country) ?? "DE";

        return new ImportOptions(
            commit,
            updateExisting,
            sendInvitations,
            settings.Region,
            settings.DateFormat,
            settings.Currency.ToString(),
            defaultCountry);
    }
}
