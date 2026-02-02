import { Component, computed, input } from "@angular/core";
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
  styleUrl: "vehicle-diagram.css",
  imports: [TooltipModule],
})
export class VehicleDiagram {
  readonly markers = input<DamageMarker[]>([]);
  readonly showLegend = input(true);

  readonly svgWidth = 440;
  readonly svgHeight = 210;

  // Diagram bounds for marker positioning
  private readonly diagramBounds = {
    minX: 60,
    maxX: 380,
    minY: 25,
    maxY: 160,
  };

  readonly markerCount = computed(() => this.markers().length);

  getMarkerX(marker: DamageMarker): number {
    const { minX, maxX } = this.diagramBounds;
    return minX + marker.x * (maxX - minX);
  }

  getMarkerY(marker: DamageMarker): number {
    const { minY, maxY } = this.diagramBounds;
    return minY + marker.y * (maxY - minY);
  }

  getSeverityColor(severity?: string): string {
    switch (severity?.toLowerCase()) {
      case "severe":
        return "#ef4444"; // red-500
      case "moderate":
        return "#f97316"; // orange-500
      case "minor":
      default:
        return "#eab308"; // yellow-500
    }
  }

  getTooltipText(marker: DamageMarker): string {
    const severity = marker.severity || "Unknown";
    const description = marker.description || "No description";
    return `${severity}: ${description}`;
  }
}
