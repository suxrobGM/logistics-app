using System.Globalization;
using System.Text;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.Documents.Pdf.VehicleBol;

/// <summary>
/// Top-view car outline (front at the top, driver's side on the left) as SVG, with every
/// <see cref="VehicleDamageZone"/> as a rectangle. The TMS portal and the driver app draw the same
/// layout from their own copy of these rectangles (viewBox 0 0 200 350) - keep the three in step.
/// </summary>
public static class VehicleDiagram
{
    public const int Width = 200;
    public const int Height = 350;

    public readonly record struct ZoneRect(int X, int Y, int W, int H);

    public static readonly IReadOnlyDictionary<VehicleDamageZone, ZoneRect> Zones =
        new Dictionary<VehicleDamageZone, ZoneRect>
        {
            [VehicleDamageZone.FrontBumper] = new(50, 10, 100, 20),
            [VehicleDamageZone.Hood] = new(50, 30, 100, 70),
            [VehicleDamageZone.Windshield] = new(50, 100, 100, 30),
            [VehicleDamageZone.Roof] = new(60, 130, 80, 110),
            [VehicleDamageZone.RearWindow] = new(50, 240, 100, 25),
            [VehicleDamageZone.Trunk] = new(50, 265, 100, 55),
            [VehicleDamageZone.RearBumper] = new(50, 320, 100, 20),

            [VehicleDamageZone.LeftSideGlass] = new(50, 130, 10, 110),
            [VehicleDamageZone.RightSideGlass] = new(140, 130, 10, 110),

            [VehicleDamageZone.LeftFrontFender] = new(20, 30, 30, 80),
            [VehicleDamageZone.LeftFrontDoor] = new(20, 110, 30, 75),
            [VehicleDamageZone.LeftRearDoor] = new(20, 185, 30, 70),
            [VehicleDamageZone.LeftRearQuarterPanel] = new(20, 255, 30, 75),

            [VehicleDamageZone.RightFrontFender] = new(150, 30, 30, 80),
            [VehicleDamageZone.RightFrontDoor] = new(150, 110, 30, 75),
            [VehicleDamageZone.RightRearDoor] = new(150, 185, 30, 70),
            [VehicleDamageZone.RightRearQuarterPanel] = new(150, 255, 30, 75),

            [VehicleDamageZone.LeftMirror] = new(5, 110, 15, 15),
            [VehicleDamageZone.RightMirror] = new(180, 110, 15, 15),

            [VehicleDamageZone.LeftFrontWheel] = new(5, 55, 15, 40),
            [VehicleDamageZone.LeftRearWheel] = new(5, 265, 15, 40),
            [VehicleDamageZone.RightFrontWheel] = new(180, 55, 15, 40),
            [VehicleDamageZone.RightRearWheel] = new(180, 265, 15, 40)
        };

    /// <summary>
    /// Codes to print per zone, e.g. Hood → "D,S". Zones without a diagram defect are absent.
    /// </summary>
    public static Dictionary<VehicleDamageZone, string> CodesByZone(IEnumerable<ConditionDefect> defects) =>
        defects
            .Where(d => d is { Zone: not null, DamageType: not null })
            .GroupBy(d => d.Zone!.Value)
            .ToDictionary(
                g => g.Key,
                g => string.Join(',', g.Select(d => d.DamageType!.Value.GetCode()).Distinct()));

    public static string BuildSvg(IEnumerable<ConditionDefect> defects)
    {
        var codes = CodesByZone(defects);
        var svg = new StringBuilder();
        svg.Append(CultureInfo.InvariantCulture,
            $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {Width} {Height}" width="{Width}" height="{Height}">""");

        foreach (var (zone, rect) in Zones)
        {
            var damaged = codes.ContainsKey(zone);
            var fill = damaged ? "#d4d4d4" : "#ffffff";
            var strokeWidth = damaged ? 2 : 1;
            svg.Append(CultureInfo.InvariantCulture,
                $"""<rect x="{rect.X}" y="{rect.Y}" width="{rect.W}" height="{rect.H}" fill="{fill}" stroke="#262626" stroke-width="{strokeWidth}"/>""");
        }

        foreach (var (zone, code) in codes)
        {
            var rect = Zones[zone];
            var x = rect.X + rect.W / 2.0;
            var y = rect.Y + rect.H / 2.0 + 4;
            svg.Append(CultureInfo.InvariantCulture,
                $"""<text x="{x}" y="{y}" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" fill="#000000">{code}</text>""");
        }

        svg.Append("</svg>");
        return svg.ToString();
    }
}
