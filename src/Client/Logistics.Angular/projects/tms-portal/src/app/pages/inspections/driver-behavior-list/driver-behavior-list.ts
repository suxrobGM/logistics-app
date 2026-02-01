import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import {
  Api,
  type DriverBehaviorEventDto,
  type DriverBehaviorEventType,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { MultiSelectModule } from "primeng/multiselect";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";

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
    ProgressSpinnerModule,
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
export class DriverBehaviorListPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly events = signal<DriverBehaviorEventDto[]>([]);

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

  async ngOnInit(): Promise<void> {
    await this.loadEvents();
  }

  private async loadEvents(): Promise<void> {
    this.isLoading.set(true);
    try {
      // TODO: Replace with actual API call once regenerated
      // const result = await this.api.invoke(getDriverBehaviorEvents, {
      //   eventTypes: this.selectedEventTypes.length > 0 ? this.selectedEventTypes : undefined,
      //   isReviewed: this.selectedReviewStatus === 'all' ? undefined : this.selectedReviewStatus === 'reviewed',
      // });
      // this.events.set(result?.data ?? []);

      // Placeholder until API is regenerated
      this.events.set([]);
      this.toastService.showInfo("Driver behavior API pending regeneration");
    } finally {
      this.isLoading.set(false);
    }
  }

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
    this.loadEvents();
  }

  protected onSearch(value: string): void {
    // Filter events locally by driver name
    // In a real implementation, this would be a server-side search
    console.log("Search:", value);
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
    if (!event) return;

    this.isReviewing.set(true);
    try {
      // TODO: Replace with actual API call once regenerated
      // await this.api.invoke(reviewDriverBehaviorEvent, {
      //   id: event.id,
      //   body: {
      //     eventId: event.id,
      //     notes: this.reviewNotes,
      //     isDismissed: this.isDismissed,
      //   },
      // });

      this.toastService.showSuccess("Event reviewed successfully");
      this.closeReviewDialog();
      await this.loadEvents();
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
