using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum EldProviderType
{
    Samsara = 1,
    Motive = 2,
    Geotab = 3,
    Omnitracs = 4,
    PeopleNet = 5,
    [Description("TT ELD")]
    TtEld = 6,
    Demo = 99
}
