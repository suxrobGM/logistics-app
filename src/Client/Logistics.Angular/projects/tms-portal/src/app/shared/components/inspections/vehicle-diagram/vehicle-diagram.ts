import { Component, input } from "@angular/core";
import { TooltipModule } from "primeng/tooltip";

export interface DamageMarker {
  x: number; // 0.0 - 1.0 position
  y: number; // 0.0 - 1.0 position
  description?: string;
  severity?: string; // Minor, Moderate, Severe
}

@Component({
  selector: "app-vehicle-diagram",
  templateUrl: "./vehicle-diagram.html",
  styleUrl: "./vehicle-diagram.css",
  imports: [TooltipModule],
})
export class VehicleDiagram {
  readonly markers = input<DamageMarker[]>([]);
  readonly showLegend = input(true);

  readonly diagramWidth = 720;
  readonly imageSrc = "/images/inspection/inspection_truck_side.png";

  private readonly vehicleBounds = {
    left: 4,
    right: 97,
    top: 31,
    bottom: 69,
  };

  getMarkerX(marker: DamageMarker): number {
    return this.mapToVehicleBounds(marker.x, this.vehicleBounds.left, this.vehicleBounds.right);
  }

  getMarkerY(marker: DamageMarker): number {
    return this.mapToVehicleBounds(marker.y, this.vehicleBounds.top, this.vehicleBounds.bottom);
  }

  getSeverityClass(severity?: string): string {
    switch (severity?.toLowerCase()) {
      case "severe":
        return "vehicle-diagram__marker--severe";
      case "moderate":
        return "vehicle-diagram__marker--moderate";
      case "minor":
      default:
        return "vehicle-diagram__marker--minor";
    }
  }

  getTooltipText(marker: DamageMarker): string {
    const severity = marker.severity || "Unknown";
    const description = marker.description || "No description";
    return `${severity}: ${description}`;
  }

  private mapToVehicleBounds(value: number, min: number, max: number): number {
    const normalized = Math.max(0, Math.min(1, value));
    return min + normalized * (max - min);
  }
}
