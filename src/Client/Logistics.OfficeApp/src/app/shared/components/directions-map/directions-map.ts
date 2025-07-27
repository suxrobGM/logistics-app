import {HttpClient} from "@angular/common/http";
import {Component, effect, inject, input, model, output, signal} from "@angular/core";
import {bbox as turfBbox} from "@turf/turf";
import {GeoJSONSourceSpecification, LngLatLike} from "mapbox-gl";
import {ImageComponent, LayerComponent, MapComponent} from "ngx-mapbox-gl";
import {environment} from "@/env";
import {GeoPoint, MapboxDirectionsResponse} from "@/shared/types/mapbox";

@Component({
  selector: "app-directions-map",
  templateUrl: "./directions-map.html",
  imports: [MapComponent, ImageComponent, LayerComponent],
})
export class DirectionsMap {
  protected readonly accessToken = environment.mapboxToken;
  private readonly defaultCenter: GeoPoint = [-95, 35];
  private readonly defaultZoom = 3;

  private readonly http = inject(HttpClient);

  /*–– state ––*/
  protected readonly route = signal<GeoJSONSourceSpecification | null>(null);
  protected readonly stopsSource = signal<GeoJSONSourceSpecification | null>(null);
  protected readonly bounds = signal<[LngLatLike, LngLatLike] | null>(null);
  protected readonly imageLoaded = signal(false);

  /*–– camera ––*/
  public readonly center = model<GeoPoint>(this.defaultCenter);
  public readonly zoom = model<number>(this.defaultZoom);

  /*–– inputs ––*/
  public readonly stops = input<GeoPoint[]>([]);
  public readonly width = input<string>("100%");
  public readonly height = input<string>("100%");

  /*–– outputs ––*/
  public readonly routeChanged = output<RouteChangedEvent>();

  constructor() {
    effect(() => this.draw());
  }

  private draw(): void {
    const pts = this.stops();

    if (!pts || pts.length < 2) {
      return this.clear();
    }

    // geojson for points
    this.stopsSource.set(this.buildStopsSource(pts));

    // polyline – Mapbox Directions with waypoints
    const coords = pts.map((p) => `${p[0]},${p[1]}`).join(";");
    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${coords}?geometries=geojson&access_token=${this.accessToken}`;

    this.http.get<MapboxDirectionsResponse>(url).subscribe((rsp) => {
      if (rsp.code !== "Ok") {
        return;
      }

      this.route.set({
        type: "geojson",
        data: {type: "Feature", geometry: rsp.routes[0].geometry, properties: {}},
      });

      const [minX, minY, maxX, maxY] = turfBbox(rsp.routes[0].geometry);
      this.bounds.set([
        [minX, minY],
        [maxX, maxY],
      ]);
      this.center.set([(minX + maxX) / 2, (minY + maxY) / 2]);

      this.routeChanged.emit({
        origin: pts[0],
        destination: pts[pts.length - 1],
        distance: rsp.routes[0].distance,
      });
    });
  }

  private buildStopsSource(pts: GeoPoint[]): GeoJSONSourceSpecification {
    const feats = pts.map((c, i) => ({
      type: "Feature",
      geometry: {type: "Point", coordinates: c},
      properties: {kind: i === 0 || i === pts.length - 1 ? "edge" : "mid"},
    }));
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    return {type: "geojson", data: {type: "FeatureCollection", features: feats as any[]}};
  }

  private clear() {
    this.route.set(null);
    this.stopsSource.set(null);
    this.bounds.set(null);
    this.center.set(this.defaultCenter);
    this.zoom.set(this.defaultZoom);
  }
}

export interface RouteChangedEvent {
  origin: number[];
  destination: number[];
  distance: number;
}
