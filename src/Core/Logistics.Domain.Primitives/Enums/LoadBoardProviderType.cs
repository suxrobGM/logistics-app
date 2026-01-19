using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum LoadBoardProviderType
{
    [Description("DAT")] [EnumMember(Value = "dat")]
    Dat = 1,

    [Description("Truckstop")] [EnumMember(Value = "truckstop")]
    Truckstop = 2,

    [Description("123Loadboard")] [EnumMember(Value = "123loadboard")]
    OneTwo3Loadboard = 3,

    [Description("Demo")] [EnumMember(Value = "demo")]
    Demo = 99
}
