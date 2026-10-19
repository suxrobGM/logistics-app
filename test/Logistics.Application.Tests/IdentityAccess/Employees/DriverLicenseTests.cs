using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Employees;

public class DriverLicenseTests
{
    private static readonly DateOnly Expiry = new(2027, 3, 1);

    [Fact]
    public void SetExpiry_Renewed_RestartsReminders()
    {
        var license = Create();
        license.RecordReminderSent(7);

        license.SetExpiry(Expiry.AddYears(4));

        Assert.Equal(Expiry.AddYears(4), license.ExpiresAt);
        Assert.Null(license.LastReminderThresholdDays);
    }

    [Fact]
    public void SetExpiry_Unchanged_KeepsReminderState()
    {
        var license = Create();
        license.RecordReminderSent(30);

        license.SetExpiry(Expiry);

        Assert.Equal(30, license.LastReminderThresholdDays);
    }

    private static DriverLicense Create() =>
        DriverLicense.Create(Guid.NewGuid(), "D123", LicenseClass.USClassA, "US", Expiry.AddYears(-4), Expiry);
}
