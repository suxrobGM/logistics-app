using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DefectSeverity
{
    [Description("Minor - Does not affect safe operation")]
    Minor,

    [Description("Major - Should be repaired soon")]
    Major,

    [Description("Out of Service - Vehicle cannot operate")]
    OutOfService
}
