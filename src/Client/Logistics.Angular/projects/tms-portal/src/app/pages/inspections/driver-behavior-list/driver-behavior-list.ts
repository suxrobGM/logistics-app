import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  Api,
  type DriverBehaviorEventDto,
  type DriverBehaviorEventType,
  reviewDriverBehaviorEvent,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { MultiSelectModule } from "primeng/multiselect";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
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
    FormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    DialogModule,
    InputTextModule,
    MenuModule,
    MultiSelectModule,
    SelectModule,
    TableModule,
    TagModule,
    TextareaModule,
    ToggleSwitchModule,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class DriverBehaviorListPage {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(DriverBehaviorListStore);

  // Filters
  protected readonly eventTypeOptions = eventTypeOptions;
  protected readonly reviewStatusOptions = reviewStatusOptions;
  protected selectedEventTypes: string[] = [];
  protected selectedReviewStatus = "all";

  // Review dialog
  protected readonly showReviewDialog = signal(false);
  protected readonly selectedEvent = signal<DriverBehaviorEventDto | null>(null);
  protected readonly isReviewing = signal(false);
  protected reviewNotes = "";
  protected isDismissed = false;

  protected getEventTypeSeverity(eventType: DriverBehaviorEventType): TagSeverity {
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

    if (this.selectedEventTypes.length === 1) {
      filters["EventType"] = this.selectedEventTypes[0];
    }

    if (this.selectedReviewStatus !== "all") {
      filters["IsReviewed"] = this.selectedReviewStatus === "reviewed";
    }

    this.store.setFilters(filters);
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected onRowClick(event: DriverBehaviorEventDto): void {
    this.openReviewDialog(event);
  }

  protected openReviewDialog(event: DriverBehaviorEventDto): void {
    this.selectedEvent.set(event);
    this.reviewNotes = event.reviewNotes ?? "";
    this.isDismissed = event.isDismissed ?? false;
    this.showReviewDialog.set(true);
  }

  protected closeReviewDialog(): void {
    this.showReviewDialog.set(false);
    this.selectedEvent.set(null);
    this.reviewNotes = "";
    this.isDismissed = false;
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
          reviewNotes: this.reviewNotes || undefined,
          isDismissed: this.isDismissed,
        },
      });

      // Optimistically update the item in the store
      this.store.updateItem(event.id, {
        isReviewed: true,
        isDismissed: this.isDismissed,
        reviewNotes: this.reviewNotes || undefined,
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
