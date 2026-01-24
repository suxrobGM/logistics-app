import { HttpClient } from "@angular/common/http";
import { Component, computed, effect, inject, input, model, output, signal } from "@angular/core";
import type { GeoPointDto } from "@logistics/shared/api/models";
import type { LineString } from "geojson";
import type { LngLatLike, MapMouseEvent } from "mapbox-gl";
import { GeoJSONSourceComponent, LayerComponent, MapComponent } from "ngx-mapbox-gl";
import { firstValueFrom } from "rxjs";
import { ThemeService } from "@/core/services";
import { environment } from "@/env";
import type { MapboxDirectionsResponse } from "@/shared/types/mapbox";
import type {
  RouteChangeEvent,
  RouteSegmentClickEvent,
  SegmentFeature,
  SegmentLayer,
  Waypoint,
  WaypointClickEvent,
  WaypointFeature,
} from "./types";

@Component({
  selector: "app-direction-map",
  templateUrl: "./direction-map.html",
  imports: [MapComponent, LayerComponent, GeoJSONSourceComponent],
})
export class DirectionMap {
  private readonly http = inject(HttpClient);
  private readonly themeService = inject(ThemeService);

  protected readonly accessToken = environment.mapboxToken;
  private readonly defaultCenter: LngLatLike = [-95, 35];
  private readonly defaultZoom = 3;
  protected readonly segmentColors = [
    "#28a745", // green
    "#0074D9", // blue
    "#FF4136", // red
    "#FF851B", // orange
    "#B10DC9", // purple
  ];

  /** Mapbox style URL based on current theme */
  protected readonly mapStyle = computed(() =>
    this.themeService.isDark()
      ? "mapbox://styles/mapbox/dark-v11"
      : "mapbox://styles/mapbox/streets-v12",
  );

  // States
  protected readonly segments = signal<SegmentLayer[]>([]);
  protected readonly waypointsData = signal<WaypointFeature | null>(null);
  protected readonly waypointHighlight = signal<WaypointFeature | null>(null); // prettier-ignore
  protected readonly segmentHighlight = signal<SegmentFeature | null>(null);
  protected readonly bounds = signal<[LngLatLike, LngLatLike] | null>(null);

  /** The center coordinates of the map. Default is [-95, 35]. */
  public readonly center = model<LngLatLike>(this.defaultCenter);

  /** The zoom level of the map. Default is 3. */
  public readonly zoom = model<number>(this.defaultZoom);

  /** Array of stop coordinates in order. */
  public readonly waypoints = input<Waypoint[]>([]);

  /** Selected waypoint for highlighting. */
  public readonly selectedWaypoint = input<Waypoint | null>(null);

  /** Color used for highlights. */
  public readonly highlightColor = input<string>("#111");

  /** The width of the map container. Default is 100%. */
  public readonly width = input<string>("100%");

  /** The height of the map container. Default is 100%. */
  public readonly height = input<string>("100%");

  /** Emitted when the route changes. */
  public readonly routeChange = output<RouteChangeEvent>();

  public readonly routeSegmentClick = output<RouteSegmentClickEvent>();
  public readonly waypointClick = output<WaypointClickEvent>();

  constructor() {
    effect(() => this.draw());

    // respond to selectedWaypoint changes
    effect(() => {
      const wp = this.selectedWaypoint();
      if (!wp) {
        return this.clearHighlight();
      }
      this.selectSegmentByWaypoint(wp.id);
    });
  }

  protected setCursor(cursor: string, evt: MapMouseEvent): void {
    evt.target.getCanvas().style.cursor = cursor;
  }

  protected onSegmentClick(_evt: MapMouseEvent, seg: SegmentLayer): void {
    this.applySegmentHighlight(seg);

    this.routeSegmentClick.emit({
      fromWaypoint: seg.fromWaypoint,
      toWaypoint: seg.toWaypoint,
    });
  }

  protected onWaypointClick(evt: MapMouseEvent): void {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const waypointId = (evt.features?.[0] as any)?.properties?.id as string | undefined;
    const waypoint = this.waypoints().find((w) => w.id === waypointId);

    if (!waypoint) {
      return;
    }
    this.waypointClick.emit({ waypoint });
    this.selectSegmentByWaypoint(waypoint.id);
  }

  /**
   * Draws the direction map.
   */
  private async draw(): Promise<void> {
    const pts = this.waypoints();

    if (!pts || pts.length < 2 || !this.coordsAreValid(pts)) {
      return this.clear();
    }

    // build waypoint FeatureCollection with labels
    this.waypointsData.set(this.toWaypointFC(pts));

    // build one LineString per leg
    const segs: SegmentLayer[] = [];
    for (let i = 0; i < pts.length - 1; i++) {
      const from = pts[i];
      const to = pts[i + 1];
      const feature = await this.buildDirectionsSegment(from.location, to.location);

      segs.push({
        layerId: `seg-${from.id}-${to.id}`,
        data: feature,
        fromWaypoint: from,
        toWaypoint: to,
      });
    }
    this.segments.set(segs);

    // Fit bounds & center
    this.fitAndCenter(pts);

    // Emit route distance (sum of legs)
    const distance = segs.reduce((sum, s) => sum + (s.data.properties?.distance ?? 0), 0);
    this.routeChange.emit({
      origin: pts[0].location,
      destination: pts.at(-1)!.location,
      distance,
    });

    // Clear any previous selection
    this.clearHighlight();
  }

  /**
   * Builds a segment for the directions map using Mapbox Directions API.
   * @param a Starting point coordinates.
   * @param b Ending point coordinates.
   * @param index Index of the segment in the route.
   * @returns A GeoJSONSourceSpecification for the segment.
   */
  private async buildDirectionsSegment(a: GeoPointDto, b: GeoPointDto): Promise<SegmentFeature> {
    const coords = `${a.longitude},${a.latitude};${b.longitude},${b.latitude}`;

    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${coords}?geometries=geojson&access_token=${this.accessToken}`;
    const res = await firstValueFrom(this.http.get<MapboxDirectionsResponse>(url));

    return {
      type: "Feature",
      geometry: res.routes?.[0]?.geometry as LineString,
      properties: { distance: res.routes?.[0]?.distance },
    };
  }

  /**
   * Fits the map to the given waypoints.
   * @param waypoints Array of waypoints.
   */
  private fitAndCenter(waypoints: Waypoint[]): void {
    // Calculate bounds
    const xs = waypoints.map((p) => p.location.longitude ?? 0);
    const ys = waypoints.map((p) => p.location.latitude ?? 0);
    const minX = Math.min(...xs);
    const minY = Math.min(...ys);
    const maxX = Math.max(...xs);
    const maxY = Math.max(...ys);

    this.bounds.set([
      [minX, minY],
      [maxX, maxY],
    ]);

    this.center.set({
      lng: (minX + maxX) / 2,
      lat: (minY + maxY) / 2,
    });
  }

  /** Prefer segment that STARTS at id; if none, take the one that ENDS at id (for last waypoint). */
  private selectSegmentByWaypoint(waypointId: string): void {
    const segs = this.segments();
    const byFrom = segs.find((s) => s.fromWaypoint.id === waypointId);
    const seg = byFrom ?? segs.find((s) => s.toWaypoint.id === waypointId);
    if (!seg) {
      return;
    }
    this.applySegmentHighlight(seg);
  }

  private applySegmentHighlight(seg: SegmentLayer): void {
    // highlight segment
    this.segmentHighlight.set(seg.data);

    // highlight endpoints
    const all = this.waypointsData()?.features ?? [];
    const selected = all.filter(
      (f) => f.properties?.id === seg.fromWaypoint.id || f.properties?.id === seg.toWaypoint.id,
    );

    // const selectedWaypoints = selected.map((f) => ({
    //   id: f.properties?.id,
    //   location: {
    //     longitude: f.geometry.coordinates[0],
    //     latitude: f.geometry.coordinates[1],
    //   },
    // }));

    // this.fitAndCenter(selectedWaypoints);

    this.waypointHighlight.set({
      type: "FeatureCollection",
      features: selected,
    });
  }

  private clearHighlight(): void {
    this.segmentHighlight.set(null);
    this.waypointHighlight.set(null);
  }

  /**
   * Clears the map and resets all state.
   */
  private clear(): void {
    this.segments.set([]);
    this.waypointsData.set(null);
    this.waypointHighlight.set(null);
    this.segmentHighlight.set(null);
    this.bounds.set(null);
    this.center.set(this.defaultCenter);
    this.zoom.set(this.defaultZoom);
  }

  /**
   * Converts an array of waypoints to a FeatureCollection.
   * @param pts Array of waypoints.
   * @returns A FeatureCollection representing the waypoints.
   */
  private toWaypointFC(pts: Waypoint[]): WaypointFeature {
    return {
      type: "FeatureCollection",
      features: pts.map((p, i) => ({
        type: "Feature",
        geometry: {
          type: "Point",
          coordinates: [p.location.longitude ?? 0, p.location.latitude ?? 0],
        },
        properties: { id: p.id, label: String(i + 1) },
      })),
    };
  }

  /**
   * Validates if the given point is a valid GeoPoint.
   * A valid GeoPoint is an array of two numbers: [longitude, latitude].
   * Longitude must be between -180 and 180, latitude must be between -90 and 90.
   * @param point The point to validate.
   * @returns True if the point is valid, false otherwise.
   */
  private isValidPoint(point: Waypoint): boolean {
    const longitude = point.location.longitude ?? 0;
    const latitude = point.location.latitude ?? 0;

    // Ignore zero coordinates
    if (longitude === 0 && latitude === 0) {
      return false;
    }

    return longitude >= -180 && longitude <= 180 && latitude >= -90 && latitude <= 90;
  }

  /** Ensures every waypoint is within valid longitude/latitude bounds. */
  private coordsAreValid(pts: Waypoint[]): boolean {
    return pts.every((pt) => this.isValidPoint(pt));
  }
}
