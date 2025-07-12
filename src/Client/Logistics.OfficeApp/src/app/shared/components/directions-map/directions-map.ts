import {HttpClient} from "@angular/common/http";
import {Component, effect, inject, input, model, output, signal} from "@angular/core";
import {GeoJSONSourceRaw, LngLatLike} from "mapbox-gl";
import {NgxMapboxGLModule} from "ngx-mapbox-gl";
import {environment} from "@/env";
import {GeoPoint, MapboxDirectionsResponse} from "@/shared/types/mapbox";

@Component({
  selector: "app-directions-map",
  templateUrl: "./directions-map.html",
  imports: [NgxMapboxGLModule],
})
export class DirectionsMap {
  protected readonly accessToken = environment.mapboxToken;
  private readonly defaultCenter: GeoPoint = [-95, 35];
  private readonly defaultZoom = 3;

  private readonly http = inject(HttpClient);

  protected readonly bounds = signal<[LngLatLike, LngLatLike] | null>(null);
  protected readonly route = signal<GeoJSONSourceRaw | null>(null);
  protected readonly startPoint = signal<GeoJSONSourceRaw | null>(null);
  protected readonly endPoint = signal<GeoJSONSourceRaw | null>(null);
  protected readonly imageLoaded = signal(false);

  public readonly center = model<GeoPoint>(this.defaultCenter);
  public readonly zoom = model<number>(this.defaultZoom);
  public readonly start = input<GeoPoint | null>(null);
  public readonly end = input<GeoPoint | null>(null);
  public readonly width = input<string>("100%");
  public readonly height = input<string>("100%");
  public readonly routeChanged = output<RouteChangedEvent>();

  constructor() {
    effect(() => {
      if (this.start() || this.end()) {
        this.drawMarkers();
        this.drawRoute();
      } else {
        this.clearRoutes();
      }
    });
  }

  private drawRoute(): void {
    const start = this.start();
    const end = this.end();
    if (!start || !end) {
      return;
    }

    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${start[0]},${start[1]};${end[0]},${end[1]}?geometries=geojson&access_token=${this.accessToken}`;

    this.http.get<MapboxDirectionsResponse>(url).subscribe((data) => {
      if (data.code !== "Ok") {
        return;
      }

      this.route.set({
        type: "geojson",
        data: {
          type: "Feature",
          geometry: data.routes[0].geometry,
          properties: {},
        },
      });
      this.bounds.set([this.start() as LngLatLike, this.end() as LngLatLike]);
      this.routeChanged.emit({
        origin: this.start()!,
        destination: this.end()!,
        distance: data.routes[0].distance,
      });
    });
  }

  private drawMarkers(): void {
    const start = this.start();
    if (start) {
      this.startPoint.set(this.getPointSource(start));
      this.center.set(start);
    }
    const end = this.end();
    if (end) {
      this.endPoint.set(this.getPointSource(end));
    }
  }

  private getPointSource(coords: number[]): GeoJSONSourceRaw {
    return {
      type: "geojson",
      data: {
        type: "Feature",
        geometry: {
          type: "Point",
          coordinates: coords,
        },
        properties: {},
      },
    };
  }

  private clearRoutes(): void {
    this.route.set(null);
    this.startPoint.set(null);
    this.endPoint.set(null);
    this.center.set(this.defaultCenter);
    this.zoom.set(this.defaultZoom);
  }
}

export interface RouteChangedEvent {
  origin: number[];
  destination: number[];
  distance: number;
}
