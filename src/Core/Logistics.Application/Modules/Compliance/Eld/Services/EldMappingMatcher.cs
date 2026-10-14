using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

internal sealed record DriverCandidate(Guid EmployeeId, string Name, string? Email, IReadOnlyCollection<string> LicenseNumbers);

internal sealed record TruckCandidate(Guid TruckId, string Number, string? Vin);

/// <summary>
/// Pairs ELD drivers and vehicles with TMS employees and trucks. A pair is only suggested when the key
/// is unique on both sides, so two "John Smith"s or a VIN typed on two trucks produce no suggestion
/// rather than a wrong one. Keys are tried strongest first and each record is used at most once.
/// </summary>
internal static class EldMappingMatcher
{
    public static List<EldDriverMappingSuggestionDto> MatchDrivers(
        IReadOnlyCollection<EldDriverDto> eldDrivers, IReadOnlyCollection<DriverCandidate> employees)
    {
        var suggestions = new List<EldDriverMappingSuggestionDto>();
        var usedExternal = new HashSet<string>();
        var usedEmployees = new HashSet<Guid>();

        void Match(EldMatchReason reason, Func<EldDriverDto, IEnumerable<string?>> eldKeys, Func<DriverCandidate, IEnumerable<string?>> tmsKeys)
        {
            foreach (var (driver, employee) in UniquePairs(
                         eldDrivers.Where(d => !usedExternal.Contains(d.ExternalDriverId)), eldKeys,
                         employees.Where(e => !usedEmployees.Contains(e.EmployeeId)), tmsKeys))
            {
                usedExternal.Add(driver.ExternalDriverId);
                usedEmployees.Add(employee.EmployeeId);
                suggestions.Add(new EldDriverMappingSuggestionDto
                {
                    ExternalDriverId = driver.ExternalDriverId,
                    ExternalDriverName = driver.Name,
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.Name,
                    MatchedBy = reason
                });
            }
        }

        Match(EldMatchReason.Email, d => [NormalizeEmail(d.Email)], e => [NormalizeEmail(e.Email)]);
        Match(EldMatchReason.LicenseNumber, d => [NormalizeCode(d.LicenseNumber)], e => e.LicenseNumbers.Select(NormalizeCode));
        Match(EldMatchReason.Name, d => [NormalizeName(d.Name)], e => [NormalizeName(e.Name)]);
        return suggestions;
    }

    public static List<EldVehicleMappingSuggestionDto> MatchVehicles(
        IReadOnlyCollection<EldVehicleDto> eldVehicles, IReadOnlyCollection<TruckCandidate> trucks)
    {
        var suggestions = new List<EldVehicleMappingSuggestionDto>();
        var usedExternal = new HashSet<string>();
        var usedTrucks = new HashSet<Guid>();

        void Match(EldMatchReason reason, Func<EldVehicleDto, IEnumerable<string?>> eldKeys, Func<TruckCandidate, IEnumerable<string?>> tmsKeys)
        {
            foreach (var (vehicle, truck) in UniquePairs(
                         eldVehicles.Where(v => !usedExternal.Contains(v.ExternalVehicleId)), eldKeys,
                         trucks.Where(t => !usedTrucks.Contains(t.TruckId)), tmsKeys))
            {
                usedExternal.Add(vehicle.ExternalVehicleId);
                usedTrucks.Add(truck.TruckId);
                suggestions.Add(new EldVehicleMappingSuggestionDto
                {
                    ExternalVehicleId = vehicle.ExternalVehicleId,
                    ExternalVehicleName = vehicle.Name,
                    TruckId = truck.TruckId,
                    TruckNumber = truck.Number,
                    MatchedBy = reason
                });
            }
        }

        Match(EldMatchReason.Vin, v => [NormalizeCode(v.Vin)], t => [NormalizeCode(t.Vin)]);
        Match(EldMatchReason.TruckNumber, VehicleNumberKeys, t => [NormalizeCode(t.Number)]);
        return suggestions;
    }

    /// <summary>
    /// A vehicle named "Truck #101" should match truck number "101": besides the whole name, a name with
    /// exactly one run of digits also offers that run as a key.
    /// </summary>
    private static IEnumerable<string?> VehicleNumberKeys(EldVehicleDto vehicle)
    {
        yield return NormalizeCode(vehicle.Name);

        var digitRuns = System.Text.RegularExpressions.Regex.Matches(vehicle.Name, @"\d+");
        if (digitRuns.Count == 1)
        {
            yield return digitRuns[0].Value;
        }
    }

    private static IEnumerable<(TLeft Left, TRight Right)> UniquePairs<TLeft, TRight>(
        IEnumerable<TLeft> left, Func<TLeft, IEnumerable<string?>> leftKeys,
        IEnumerable<TRight> right, Func<TRight, IEnumerable<string?>> rightKeys)
    {
        var leftByKey = Index(left, leftKeys);
        var rightByKey = Index(right, rightKeys);
        var pairedLeft = new HashSet<TLeft>();
        var pairedRight = new HashSet<TRight>();

        foreach (var (key, lefts) in leftByKey)
        {
            if (lefts.Count != 1 || !rightByKey.TryGetValue(key, out var rights) || rights.Count != 1)
            {
                continue;
            }

            var (l, r) = (lefts[0], rights[0]);
            if (pairedLeft.Add(l) && pairedRight.Add(r))
            {
                yield return (l, r);
            }
        }
    }

    private static Dictionary<string, List<T>> Index<T>(IEnumerable<T> items, Func<T, IEnumerable<string?>> keys)
    {
        var index = new Dictionary<string, List<T>>();
        foreach (var item in items)
        {
            foreach (var key in keys(item).Where(k => !string.IsNullOrEmpty(k)).Distinct())
            {
                if (!index.TryGetValue(key!, out var list))
                {
                    index[key!] = list = [];
                }

                list.Add(item);
            }
        }

        return index;
    }

    private static string? NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant();

    private static string? NormalizeCode(string? value) =>
        value is null ? null : new string(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static string? NormalizeName(string? name) =>
        name is null
            ? null
            : string.Join(' ', name.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => new string(part.Where(char.IsLetter).ToArray()))
                .Where(part => part.Length > 0));
}
