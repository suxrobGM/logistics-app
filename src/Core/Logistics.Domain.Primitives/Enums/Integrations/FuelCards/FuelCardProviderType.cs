using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum FuelCardProviderType
{
    [Description("WEX")]
    Wex = 1,

    [Description("EFS")]
    Efs = 2,

    Comdata = 3,

    Demo = 99
}
