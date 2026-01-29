using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum EmergencyContactType
{
    [Description("Safety Manager")] [EnumMember(Value = "safety_manager")]
    SafetyManager,

    [Description("Dispatcher")] [EnumMember(Value = "dispatcher")]
    Dispatcher,

    [Description("Fleet Manager")] [EnumMember(Value = "fleet_manager")]
    FleetManager,

    [Description("Emergency Services")] [EnumMember(Value = "emergency_services")]
    EmergencyServices,

    [Description("Family Member")] [EnumMember(Value = "family_member")]
    FamilyMember,

    [Description("Insurance Company")] [EnumMember(Value = "insurance")]
    Insurance,

    [Description("Tow Service")] [EnumMember(Value = "tow_service")]
    TowService
}
