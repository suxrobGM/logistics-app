using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Who supplies the chassis a container rides on for a move.
/// </summary>
public enum ChassisProvider
{
    /// <summary>The carrier's own chassis - no rental charge.</summary>
    Own,

    [Description("DCLI")]
    Dcli,

    [Description("TRAC Intermodal")]
    Trac,

    [Description("Flexi-Van")]
    FlexiVan,

    Other
}
