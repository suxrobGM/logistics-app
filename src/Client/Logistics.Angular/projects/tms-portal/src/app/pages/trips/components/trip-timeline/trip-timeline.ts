import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import type { TripTimelineEventDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { TimelineModule } from "primeng/timeline";

@Component({
  selector: "app-trip-timeline",
  templateUrl: "./trip-timeline.html",
  imports: [CommonModule, TimelineModule, CardModule, TagModule],
})
export class TripTimeline {
  public readonly events = input<TripTimelineEventDto[]>([]);

  protected getEventIcon(eventType: string | null): string {
    switch (eventType) {
      case "created":
        return "pi pi-plus";
      case "dispatched":
        return "pi pi-send";
      case "pickup":
        return "pi pi-upload";
      case "delivery":
        return "pi pi-download";
      case "completed":
        return "pi pi-check";
      case "cancelled":
        return "pi pi-times";
      default:
        return "pi pi-circle";
    }
  }

  protected getEventLabel(eventType: string | null): string {
    switch (eventType) {
      case "created":
        return "Created";
      case "dispatched":
        return "Dispatched";
      case "pickup":
        return "Pick Up";
      case "delivery":
        return "Delivery";
      case "completed":
        return "Completed";
      case "cancelled":
        return "Cancelled";
      default:
        return eventType ?? "Unknown";
    }
  }

  protected getEventSeverity(
    eventType: string | null,
  ): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" | undefined {
    switch (eventType) {
      case "created":
        return "info";
      case "dispatched":
        return "info";
      case "pickup":
        return "warn";
      case "delivery":
        return "success";
      case "completed":
        return "success";
      case "cancelled":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected getMarkerClass(eventType: string | null): string {
    switch (eventType) {
      case "created":
        return "bg-blue-500 text-white";
      case "dispatched":
        return "bg-blue-600 text-white";
      case "pickup":
        return "bg-orange-500 text-white";
      case "delivery":
        return "bg-green-500 text-white";
      case "completed":
        return "bg-green-600 text-white";
      case "cancelled":
        return "bg-red-500 text-white";
      default:
        return "bg-gray-500 text-white";
    }
  }
}
