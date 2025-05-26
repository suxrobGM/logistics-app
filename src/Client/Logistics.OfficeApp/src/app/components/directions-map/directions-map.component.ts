import {HttpClient} from "@angular/common/http";
import {Component, Input, OnChanges, input, output} from "@angular/core";
import {GeoJSONSourceRaw, LngLatLike} from "mapbox-gl";
import {NgxMapboxGLModule} from "ngx-mapbox-gl";

@Component({
  selector: "app-directions-map",
  templateUrl: "./directions-map.component.html",
  imports: [NgxMapboxGLModule],
})
export class DirectionsMapComponent implements OnChanges {
  public bounds?: [LngLatLike, LngLatLike] | null;
  public route?: GeoJSONSourceRaw | null;
  public startPoint?: GeoJSONSourceRaw | null;
  public endPoint?: GeoJSONSourceRaw | null;
  public isImageLoaded: boolean;
  private defaultCenter: [number, number];
  private defaultZoom: number;

  public readonly accessToken = input.required<string>();
  @Input() center: [number, number];
  @Input() zoom: number;
  public readonly start = input<[number, number] | null>();
  public readonly end = input<[number, number] | null>();
  @Input() width: string;
  @Input() height: string;
  public readonly routeChanged = output<RouteChangedEvent>();

  constructor(private http: HttpClient) {
    this.zoom = this.defaultZoom = 3;
    this.center = this.defaultCenter = [-95, 35];
    this.isImageLoaded = false;
    this.route = null;
    this.startPoint = null;
    this.endPoint = null;
    this.bounds = null;
    this.width = "100%";
    this.height = "100%";
  }

  ngOnChanges() {
    if (!this.start() && !this.end()) {
      this.clearRoutes();
    } else {
      this.drawMarkers();
      this.drawRoute();
    }
  }

  private drawRoute() {
    const start = this.start();
    const end = this.end();
    if (!start || !end) {
      return;
    }

    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${start[0]},${start[1]};${end[0]},${end[1]}?geometries=geojson&access_token=${this.accessToken()}`;

    this.http.get<ResponseData>(url).subscribe((data) => {
      if (data.code !== "Ok") {
        return;
      }

      this.route = {
        type: "geojson",
        data: {
          type: "Feature",
          geometry: data.routes[0].geometry,
          properties: {},
        },
      };
      this.bounds = [this.start() as LngLatLike, this.end() as LngLatLike];
      this.routeChanged.emit({
        origin: this.start()!,
        destination: this.end()!,
        distance: data.routes[0].distance,
      });
    });
  }

  private drawMarkers() {
    const start = this.start();
    if (start) {
      this.startPoint = this.getPointSource(start);
      this.center = start;
    }
    const end = this.end();
    if (end) {
      this.endPoint = this.getPointSource(end);
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

  private clearRoutes() {
    this.route = null;
    this.startPoint = null;
    this.endPoint = null;
    this.center = this.defaultCenter;
    this.zoom = this.defaultZoom;
  }
}

interface ResponseData {
  code: string;
  routes: RouteData[];
}

interface RouteData {
  duration: number;
  distance: number;
  weight_name: string;
  weight: number;
  duration_typical: number;
  weight_typical: number;
  geometry: GeoJSON.Geometry;
}

export interface RouteChangedEvent {
  origin: number[];
  destination: number[];
  distance: number;
}
