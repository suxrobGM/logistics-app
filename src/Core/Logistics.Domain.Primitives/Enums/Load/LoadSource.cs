using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// How a load entered the system.
/// </summary>
public enum LoadSource
{
    Manual,
    Email,

    [Description("PDF")]
    Pdf,

    [Description("API")]
    Api,

    LoadBoard
}
