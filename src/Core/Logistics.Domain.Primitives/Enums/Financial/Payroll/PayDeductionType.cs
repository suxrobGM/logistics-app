using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Kind of recurring deduction taken from every settlement of an (owner-operator) driver.
/// </summary>
public enum PayDeductionType
{
    TruckLease,
    Insurance,

    [Description("ELD fee")]
    EldFee,

    /// <summary>Held back into the driver's escrow balance rather than kept by the carrier.</summary>
    Escrow,

    Other
}
