using System.Text.Json;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Eld;

/// <summary>Payloads are the documented examples from developers.samsara.com, trimmed to the fields we read.</summary>
public class SamsaraMapperTests
{
    internal const string HosClocksJson = """
        {
          "data": [
            {
              "driver": { "id": "88668", "name": "Susan Bob" },
              "currentVehicle": { "id": "123456789", "name": "Midwest Truck #4", "ExternalIds": { "maintenanceId": "250020" } },
              "currentDutyStatus": { "hosStatusType": "driving" },
              "clocks": {
                "drive": { "driveRemainingDurationMs": 39600000 },
                "shift": { "shiftRemainingDurationMs": 50400000 },
                "cycle": { "cycleRemainingDurationMs": 252000000, "cycleTomorrowDurationMs": 252000000, "cycleStartedAtTime": "2020-01-27T07:06:25Z" },
                "break": { "timeUntilBreakDurationMs": 28800000 }
              },
              "violations": { "shiftDrivingViolationDurationMs": 0, "cycleViolationDurationMs": 0 }
            }
          ],
          "pagination": { "endCursor": "MjkY", "hasNextPage": false }
        }
        """;

    private const string HosLogsJson = """
        {
          "data": [
            {
              "driver": { "id": "88668", "name": "Susan Bob" },
              "hosLogs": [
                {
                  "logStartTime": "2020-01-27T07:06:25Z",
                  "logEndTime": "2020-01-27T08:36:25Z",
                  "hosStatusType": "offDuty",
                  "logRecordedLocation": { "latitude": 37.7749, "longitude": -122.4194 },
                  "remark": "Lunch Break",
                  "vehicle": { "id": "123456789", "name": "Midwest Truck #4" },
                  "codrivers": []
                }
              ]
            }
          ],
          "pagination": { "endCursor": "", "hasNextPage": false }
        }
        """;

    private const string ViolationsJson = """
        {
          "data": [
            {
              "violations": [
                {
                  "type": "shiftHours",
                  "description": "Daily Off-Duty Time (Canada South-10 hours)",
                  "durationMs": 31970000,
                  "violationStartTime": "2019-06-13T19:08:25Z",
                  "day": { "startTime": "2019-06-13T12:00:00Z", "endTime": "2019-06-14T12:00:00Z" },
                  "driver": { "id": "45646", "name": "Driver Bob", "externalIds": {} }
                }
              ]
            }
          ],
          "pagination": { "endCursor": "MjkY", "hasNextPage": false }
        }
        """;

    private const string VehiclesJson = """
        {
          "data": [
            {
              "id": "112",
              "name": "Truck A7",
              "vin": "1FUJA6BD31LJ09646",
              "licensePlate": "6SAM123",
              "make": "Ford",
              "model": "F150",
              "year": "2008",
              "serial": "GFRV-43N-VGX",
              "externalIds": { "maintenanceId": "250020" }
            }
          ],
          "pagination": { "endCursor": "", "hasNextPage": false }
        }
        """;

    private const string VehicleStatsJson = """
        {
          "data": [
            {
              "id": "112",
              "name": "Truck A7",
              "gps": {
                "time": "2020-01-27T07:06:25Z",
                "latitude": 37.7749,
                "longitude": -122.4194,
                "headingDegrees": 120,
                "speedMilesPerHour": 48.3,
                "isEcuSpeed": true,
                "reverseGeo": { "formattedLocation": "16 N Fair Oaks Ave, Pasadena, CA 91103" }
              },
              "obdOdometerMeters": { "time": "2020-01-27T07:06:25Z", "value": 14010293 }
            },
            { "id": "113", "name": "Parked trailer tractor" }
          ],
          "pagination": { "endCursor": "", "hasNextPage": false }
        }
        """;

    private const string DriverCreatedJson = """
        {
          "eventId": "017db07f-6e95-470e-8cc0-a371f9deed2b",
          "eventTime": "1970-01-20T06:39:05.683Z",
          "eventType": "DriverCreated",
          "orgId": 20936,
          "webhookId": "1411751028848270",
          "data": { "driver": { "id": "45646", "name": "Driver Bob", "licenseNumber": "B0BDR1V" } }
        }
        """;

    private const string HosViolationAlertJson = """
        {
          "eventId": "017db07f-6e95-470e-8cc0-a371f9deed2b",
          "eventTime": "1970-01-20T06:39:05.683Z",
          "eventType": "AlertIncident",
          "orgId": 20936,
          "webhookId": "1411751028848270",
          "data": {
            "conditions": [
              {
                "description": "Alert Type Description",
                "details": { "hosViolation": { "driver": { "id": "45646", "name": "Driver Bob" } } },
                "triggerId": 1000
              }
            ],
            "happenedAtTime": "2019-06-13T19:08:25Z",
            "isResolved": false
          }
        }
        """;

    private const string PingJson = """
        { "eventId": "8928d556-72f5-439c-a924-953022c44bfb", "eventMs": 1589474494538, "eventType": "Ping", "event": { "text": "Ping" } }
        """;

    #region HOS clocks

    [Fact]
    public void MapToDto_DocumentedClock_ReadsNestedClocks()
    {
        var clock = Page<SamsaraHosClock>(HosClocksJson).Single();

        var dto = SamsaraMapper.MapToDto(clock)!;

        Assert.Equal("88668", dto.ExternalDriverId);
        Assert.Equal("Susan Bob", dto.ExternalDriverName);
        Assert.Equal(DutyStatus.Driving, dto.CurrentDutyStatus);
        Assert.Equal(660, dto.DrivingMinutesRemaining);
        Assert.Equal(840, dto.OnDutyMinutesRemaining);
        Assert.Equal(4200, dto.CycleMinutesRemaining);
        Assert.Equal(TimeSpan.FromHours(8), dto.TimeUntilBreakRequired);
        Assert.False(dto.IsInViolation);
        Assert.Null(dto.StatusChangedAt);
    }

    [Fact]
    public void MapToDto_ViolationDurationPresent_FlagsViolation()
    {
        var clock = Page<SamsaraHosClock>(HosClocksJson).Single() with
        {
            Violations = new SamsaraClockViolations(39600000, 0)
        };

        Assert.True(SamsaraMapper.MapToDto(clock)!.IsInViolation);
    }

    [Fact]
    public void MapToDto_DisconnectedDriverApp_ReturnsNull()
    {
        var clock = Page<SamsaraHosClock>(HosClocksJson).Single() with
        {
            CurrentDutyStatus = new SamsaraDutyStatus("")
        };

        Assert.Null(SamsaraMapper.MapToDto(clock));
    }

    [Theory]
    [InlineData("offDuty", DutyStatus.OffDuty)]
    [InlineData("sleeperBed", DutyStatus.SleeperBerth)]
    [InlineData("driving", DutyStatus.Driving)]
    [InlineData("onDuty", DutyStatus.OnDutyNotDriving)]
    [InlineData("yardMove", DutyStatus.YardMove)]
    [InlineData("personalConveyance", DutyStatus.PersonalConveyance)]
    public void MapDutyStatus_DocumentedValues_MapToDomain(string input, DutyStatus expected)
    {
        Assert.Equal(expected, SamsaraMapper.MapDutyStatus(input));
    }

    [Theory]
    [InlineData("off_duty")]
    [InlineData("")]
    [InlineData(null)]
    public void MapDutyStatus_UndocumentedValue_ReturnsNullInsteadOfOffDuty(string? input)
    {
        Assert.Null(SamsaraMapper.MapDutyStatus(input));
    }

    #endregion

    #region HOS logs and violations

    [Fact]
    public void MapToLogDto_DocumentedLog_ReadsTimesLocationAndRemark()
    {
        var driverLogs = Page<SamsaraDriverHosLogs>(HosLogsJson).Single();

        var dto = SamsaraMapper.MapToLogDto(driverLogs.Driver!.Id!, driverLogs.HosLogs!.Single())!;

        Assert.Equal(DutyStatus.OffDuty, dto.DutyStatus);
        Assert.Equal(new DateTime(2020, 1, 27, 7, 6, 25, DateTimeKind.Utc), dto.StartTime);
        Assert.Equal(DateTimeKind.Utc, dto.StartTime.Kind);
        Assert.Equal(90, dto.DurationMinutes);
        Assert.Equal(new DateTime(2020, 1, 27), dto.LogDate);
        Assert.Equal(37.7749, dto.Latitude);
        Assert.Equal("Lunch Break", dto.Remark);
        Assert.Equal("88668:1580108785000", dto.ExternalLogId);
    }

    [Fact]
    public void MapToViolationDto_DocumentedViolation_UsesTypeDescriptionAndStableId()
    {
        var violation = Page<SamsaraDriverViolations>(ViolationsJson).Single().Violations!.Single();

        var dto = SamsaraMapper.MapToViolationDto(violation)!;

        Assert.Equal("45646", dto.ExternalDriverId);
        Assert.Equal(HosViolationType.OnDuty14Hour, dto.ViolationType);
        Assert.Equal("Daily Off-Duty Time (Canada South-10 hours)", dto.Description);
        Assert.Equal(new DateTime(2019, 6, 13, 19, 8, 25, DateTimeKind.Utc), dto.ViolationDate);
        Assert.Equal(4, dto.SeverityLevel);
        Assert.Equal("45646:shiftHours:1560452905000", dto.ExternalViolationId);
    }

    [Theory]
    [InlineData("dailyDrivingHours", HosViolationType.Driving11Hour)]
    [InlineData("shiftDrivingHours", HosViolationType.Driving11Hour)]
    [InlineData("shiftOnDutyHours", HosViolationType.OnDuty14Hour)]
    [InlineData("restbreakMissed", HosViolationType.Break30Minute)]
    [InlineData("cycleHoursOn", HosViolationType.Cycle70Hour)]
    [InlineData("mandatory24HoursOffDuty", HosViolationType.RestartRequired)]
    [InlineData("unsubmittedLogs", HosViolationType.FormAndMannerViolation)]
    public void MapViolationType_DocumentedTypes_MapToDomain(string input, HosViolationType expected)
    {
        Assert.Equal(expected, SamsaraMapper.MapViolationType(input));
    }

    [Fact]
    public void MapViolationType_None_ReturnsNull()
    {
        Assert.Null(SamsaraMapper.MapViolationType("NONE"));
    }

    #endregion

    #region Vehicles and GPS

    [Fact]
    public void MapToVehicleDto_YearSentAsString_ParsesToInt()
    {
        var dto = SamsaraMapper.MapToVehicleDto(Page<SamsaraVehicle>(VehiclesJson).Single());

        Assert.Equal(2008, dto.Year);
        Assert.Equal("1FUJA6BD31LJ09646", dto.Vin);
        Assert.Equal("Truck A7", dto.Name);
    }

    [Fact]
    public void MapToLocationDto_DocumentedStats_ReadsGpsAndOdometerInMiles()
    {
        var stats = Page<SamsaraVehicleStats>(VehicleStatsJson);

        var dto = SamsaraMapper.MapToLocationDto(stats[0])!;

        Assert.Equal("112", dto.ExternalVehicleId);
        Assert.Equal(37.7749, dto.Latitude);
        Assert.Equal(-122.4194, dto.Longitude);
        Assert.Equal(new DateTime(2020, 1, 27, 7, 6, 25, DateTimeKind.Utc), dto.Timestamp);
        Assert.Equal(8705, dto.OdometerMiles);
    }

    [Fact]
    public void MapToLocationDto_NoGpsFix_ReturnsNull()
    {
        Assert.Null(SamsaraMapper.MapToLocationDto(Page<SamsaraVehicleStats>(VehicleStatsJson)[1]));
    }

    #endregion

    #region Webhooks

    [Fact]
    public void MapToWebhookResult_DriverCreated_CarriesEventIdAndDriver()
    {
        var result = SamsaraMapper.MapToWebhookResult(Parse<SamsaraWebhookPayload>(DriverCreatedJson));

        Assert.Equal(EldWebhookEventType.DriverCreated, result.EventType);
        Assert.Equal("017db07f-6e95-470e-8cc0-a371f9deed2b", result.ExternalEventId);
        Assert.Equal("45646", result.ExternalDriverId);
    }

    [Fact]
    public void MapToWebhookResult_HosViolationAlert_IsViolationCreatedForDriver()
    {
        var result = SamsaraMapper.MapToWebhookResult(Parse<SamsaraWebhookPayload>(HosViolationAlertJson));

        Assert.Equal(EldWebhookEventType.ViolationCreated, result.EventType);
        Assert.Equal("45646", result.ExternalDriverId);
    }

    [Fact]
    public void MapToWebhookResult_ResolvedHosViolationAlert_IsViolationResolved()
    {
        var payload = Parse<SamsaraWebhookPayload>(HosViolationAlertJson);
        payload = payload with { Data = payload.Data! with { IsResolved = true } };

        Assert.Equal(EldWebhookEventType.ViolationResolved, SamsaraMapper.MapToWebhookResult(payload).EventType);
    }

    [Fact]
    public void MapToWebhookResult_Ping_IsValidUnknown()
    {
        var result = SamsaraMapper.MapToWebhookResult(Parse<SamsaraWebhookPayload>(PingJson));

        Assert.True(result.IsValid);
        Assert.Equal(EldWebhookEventType.Unknown, result.EventType);
        Assert.Equal("8928d556-72f5-439c-a924-953022c44bfb", result.ExternalEventId);
    }

    #endregion

    private static List<T> Page<T>(string json) => Parse<SamsaraPage<T>>(json).Data!;

    private static T Parse<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, IntegrationJsonOptions.CamelCase)!;
}
