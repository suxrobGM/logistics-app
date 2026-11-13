using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum LoadBoardProviderType
{
    [Description("DAT")]
    Dat = 1,

    Truckstop = 2,

    [Description("123Loadboard")]
    OneTwo3Loadboard = 3,

    Demo = 99
}
