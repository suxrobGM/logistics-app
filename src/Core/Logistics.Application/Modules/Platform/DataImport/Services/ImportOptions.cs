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
    string DefaultCountry);
