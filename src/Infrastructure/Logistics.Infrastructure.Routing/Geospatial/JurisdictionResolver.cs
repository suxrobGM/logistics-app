using System.Reflection;
using System.Text.Json;
using Logistics.Application.Abstractions.Geocoding;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Converters;

namespace Logistics.Infrastructure.Routing.Geospatial;

/// <summary>
/// Offline point-in-polygon jurisdiction lookup over Natural Earth admin-1 boundaries
/// (embedded resource). Boundaries load once into an STRtree of prepared geometries —
/// lookups are microseconds, so the 5-minute ELD ping path stays cheap. Registered as a
/// singleton.
/// </summary>
internal sealed class JurisdictionResolver : IJurisdictionResolver
{
    private const string ResourceName = "Logistics.Infrastructure.Routing.Data.admin1-boundaries.geojson";

    private readonly STRtree<(IPreparedGeometry Geometry, TaxJurisdiction Jurisdiction)> index = new();
    private readonly GeometryFactory geometryFactory = new();

    public JurisdictionResolver(ILogger<JurisdictionResolver> logger)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded boundary resource '{ResourceName}' not found");

        var options = new JsonSerializerOptions();
        options.Converters.Add(new GeoJsonConverterFactory());
        var collection = JsonSerializer.Deserialize<FeatureCollection>(stream, options)
            ?? throw new InvalidOperationException("Failed to parse boundary GeoJSON");

        var count = 0;
        foreach (var feature in collection)
        {
            var country = feature.Attributes["country"]?.ToString();
            var region = feature.Attributes["region"]?.ToString();
            if (feature.Geometry is null || string.IsNullOrEmpty(country) || string.IsNullOrEmpty(region))
            {
                continue;
            }

            var jurisdiction = TaxJurisdiction.Create(country, region);
            index.Insert(
                feature.Geometry.EnvelopeInternal,
                (PreparedGeometryFactory.Prepare(feature.Geometry), jurisdiction));
            count++;
        }

        index.Build();
        logger.LogInformation("Loaded {Count} jurisdiction boundary polygons", count);
    }

    public TaxJurisdiction? Resolve(GeoPoint point)
    {
        var ntsPoint = geometryFactory.CreatePoint(new Coordinate(point.Longitude, point.Latitude));

        foreach (var (geometry, jurisdiction) in index.Query(ntsPoint.EnvelopeInternal))
        {
            if (geometry.Covers(ntsPoint))
            {
                return jurisdiction;
            }
        }

        return null;
    }
}
