import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import { Icon, Stack, Typography, type IconName } from "@logistics/shared/ui";

interface TimelineStep {
  label: string;
  date: string | null | undefined;
  icon: IconName;
}

/**
 * Horizontal timeline showing shipment progress (Dispatched → Picked Up → Delivered).
 */
@Component({
  selector: "cp-shipment-timeline",
  templateUrl: "./shipment-timeline.html",
  imports: [DatePipe, Icon, Stack, Typography],
})
export class ShipmentTimeline {
  public readonly dispatchedAt = input<string | null | undefined>(null);
  public readonly pickedUpAt = input<string | null | undefined>(null);
  public readonly deliveredAt = input<string | null | undefined>(null);

  protected readonly steps = computed<TimelineStep[]>(() => [
    { label: "Dispatched", date: this.dispatchedAt(), icon: "send" },
    { label: "Picked Up", date: this.pickedUpAt(), icon: "box" },
    { label: "Delivered", date: this.deliveredAt(), icon: "circle-check" },
  ]);

  protected getStepColor(step: TimelineStep): string {
    return step.date ? "var(--success)" : "var(--border)";
  }
}
