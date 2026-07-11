import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import {
  Api,
  reviewDriverBehaviorEvent,
  type DriverBehaviorEventDto,
  type DriverBehaviorEventType,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Grid,
  Icon,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiDialog,
  UiMultiSelectField,
  UiSelectField,
  UiSortHeader,
  UiTextareaField,
  UiToggleField,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchField } from "@/shared/components";
import { DriverBehaviorListStore } from "../store";

const eventTypeOptions = [
  { label: "Harsh Braking", value: "harsh_braking" },
  { label: "Harsh Acceleration", value: "harsh_acceleration" },
  { label: "Harsh Cornering", value: "harsh_cornering" },
  { label: "Speeding", value: "speeding" },
  { label: "Distracted Driving", value: "distracted_driving" },
  { label: "Drowsiness Detected", value: "drowsiness" },
  { label: "Tailgating", value: "tailgating" },
  { label: "Rolling Stop", value: "rolling_stop" },
  { label: "Cell Phone Use", value: "cell_phone_use" },
  { label: "Seatbelt Violation", value: "seatbelt_violation" },
  { label: "Camera Obstruction", value: "camera_obstruction" },
  { label: "Forward Collision Warning", value: "forward_collision_warning" },
  { label: "Lane Departure Warning", value: "lane_departure_warning" },
];

const reviewStatusOptions = [
  { label: "All", value: "all" },
  { label: "Unreviewed", value: "unreviewed" },
  { label: "Reviewed", value: "reviewed" },
];

@Component({
  selector: "app-driver-behavior-list",
  templateUrl: "./driver-behavior-list.html",
  providers: [DriverBehaviorListStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    DatePipe,
    DecimalPipe,
    Grid,
    Icon,
    PageHeader,
    SearchField,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiDialog,
    UiMultiSelectField,
    UiSelectField,
    UiSortHeader,
    UiTextareaField,
    UiToggleField,
  ],
})
export class DriverBehaviorListPage {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(DriverBehaviorListStore);

  // Filters
  protected readonly eventTypeOptions = eventTypeOptions;
  protected readonly reviewStatusOptions = reviewStatusOptions;
  protected readonly selectedEventTypes = signal<string[]>([]);
  protected readonly selectedReviewStatus = signal<string | null>("all");

  // Review dialog
  protected readonly showReviewDialog = signal(false);
  protected readonly selectedEvent = signal<DriverBehaviorEventDto | null>(null);
  protected readonly isReviewing = signal(false);
  protected readonly reviewNotes = signal("");
  protected readonly isDismissed = signal(false);

  protected getEventTypeSeverity(eventType: DriverBehaviorEventType): UiBadgeIntent {
    switch (eventType) {
      case "speeding":
        return "warn";
      case "harsh_braking":
      case "harsh_acceleration":
      case "harsh_cornering":
        return "info";
      case "distracted_driving":
      case "cell_phone_use":
      case "drowsiness":
        return "danger";
      case "tailgating":
      case "forward_collision_warning":
      case "lane_departure_warning":
        return "warn";
      default:
        return "secondary";
    }
  }

  protected onFilterChange(): void {
    const filters: Record<string, unknown> = {};

    if (this.selectedEventTypes().length === 1) {
      filters["EventType"] = this.selectedEventTypes()[0];
    }

    if (this.selectedReviewStatus() !== "all") {
      filters["IsReviewed"] = this.selectedReviewStatus() === "reviewed";
    }

    this.store.setFilters(filters);
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected openReviewDialog(event: DriverBehaviorEventDto): void {
    this.selectedEvent.set(event);
    this.reviewNotes.set(event.reviewNotes ?? "");
    this.isDismissed.set(event.isDismissed ?? false);
    this.showReviewDialog.set(true);
  }

  protected closeReviewDialog(): void {
    this.showReviewDialog.set(false);
    this.selectedEvent.set(null);
    this.reviewNotes.set("");
    this.isDismissed.set(false);
  }

  protected async submitReview(): Promise<void> {
    const event = this.selectedEvent();
    if (!event?.id) return;

    this.isReviewing.set(true);
    try {
      await this.api.invoke(reviewDriverBehaviorEvent, {
        id: event.id,
        body: {
          id: event.id,
          reviewNotes: this.reviewNotes() || undefined,
          isDismissed: this.isDismissed(),
        },
      });

      // Optimistically update the item in the store
      this.store.updateItem(event.id, {
        isReviewed: true,
        isDismissed: this.isDismissed(),
        reviewNotes: this.reviewNotes() || undefined,
      });

      this.toastService.showSuccess("Event reviewed successfully");
      this.closeReviewDialog();
    } catch {
      this.toastService.showError("Failed to review event");
    } finally {
      this.isReviewing.set(false);
    }
  }

  protected getSpeedDisplay(event: DriverBehaviorEventDto): string {
    if (event.speedMph == null) return "-";
    let display = `${event.speedMph} mph`;
    if (event.speedLimitMph != null) {
      display += ` (limit: ${event.speedLimitMph} mph)`;
    }
    return display;
  }
}
