using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DefectSeverity
{
    [Description("Minor - Does not affect safe operation")] [EnumMember(Value = "minor")]
    Minor,

    [Description("Major - Should be repaired soon")] [EnumMember(Value = "major")]
    Major,

    [Description("Out of Service - Vehicle cannot operate")] [EnumMember(Value = "out_of_service")]
    OutOfService
}
