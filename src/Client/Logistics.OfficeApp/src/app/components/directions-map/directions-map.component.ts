import {Component, Input, OnChanges, Output, EventEmitter} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {NgxMapboxGLModule} from "ngx-mapbox-gl";
import {GeoJSONSourceRaw, LngLatLike} from "mapbox-gl";

@Component({
  selector: "app-directions-map",
  standalone: true,
  templateUrl: "./directions-map.component.html",
  styleUrls: ["./directions-map.component.scss"],
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

  @Input({required: true}) accessToken!: string;
  @Input() center: [number, number];
  @Input() zoom: number;
  @Input() start?: [number, number] | null;
  @Input() end?: [number, number] | null;
  @Input() width: string;
  @Input() height: string;
  @Output() routeChanged = new EventEmitter<RouteChangedEvent>();

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
    if (!this.start && !this.end) {
      this.clearRoutes();
    } else {
      this.drawMarkers();
      this.drawRoute();
    }
  }

  private drawRoute() {
    if (!this.start || !this.end) {
      return;
    }

    const url = `https://api.mapbox.com/directions/v5/mapbox/driving/${this.start[0]},${this.start[1]};${this.end[0]},${this.end[1]}?geometries=geojson&access_token=${this.accessToken}`;

    this.http.get<ResponseData>(url).subscribe((data) => {
      if (data.code !== "Ok") {
        return;
      }

      (this.route = {
        type: "geojson",
        data: {
          type: "Feature",
          geometry: data.routes[0].geometry,
          properties: {},
        },
      }),
        (this.bounds = [this.start as LngLatLike, this.end as LngLatLike]);
      this.routeChanged.emit({
        origin: this.start!,
        destination: this.end!,
        distance: data.routes[0].distance,
      });
    });
  }

  private drawMarkers() {
    if (this.start) {
      this.startPoint = this.getPointSource(this.start);
      this.center = this.start;
    }
    if (this.end) {
      this.endPoint = this.getPointSource(this.end);
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
