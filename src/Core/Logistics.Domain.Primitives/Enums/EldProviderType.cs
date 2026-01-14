using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum EldProviderType
{
    [Description("Samsara")] [EnumMember(Value = "samsara")]
    Samsara = 1,

    [Description("Motive")] [EnumMember(Value = "motive")]
    Motive = 2,

    [Description("Geotab")] [EnumMember(Value = "geotab")]
    Geotab = 3,

    [Description("Omnitracs")] [EnumMember(Value = "omnitracs")]
    Omnitracs = 4,

    [Description("PeopleNet")] [EnumMember(Value = "people_net")]
    PeopleNet = 5,

    [Description("Demo")] [EnumMember(Value = "demo")]
    Demo = 99
}
