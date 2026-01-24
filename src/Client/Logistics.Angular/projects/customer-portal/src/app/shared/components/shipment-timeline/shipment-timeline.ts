import { DatePipe } from "@angular/common";
import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";

interface TimelineStep {
  label: string;
  date: string | null | undefined;
  icon: string;
}

/**
 * Horizontal timeline showing shipment progress (Dispatched → Picked Up → Delivered).
 */
@Component({
  selector: "cp-shipment-timeline",
  templateUrl: "./shipment-timeline.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
})
export class ShipmentTimeline {
  public readonly dispatchedAt = input<string | null | undefined>(null);
  public readonly pickedUpAt = input<string | null | undefined>(null);
  public readonly deliveredAt = input<string | null | undefined>(null);

  protected readonly steps = computed<TimelineStep[]>(() => [
    { label: "Dispatched", date: this.dispatchedAt(), icon: "pi pi-send" },
    { label: "Picked Up", date: this.pickedUpAt(), icon: "pi pi-box" },
    { label: "Delivered", date: this.deliveredAt(), icon: "pi pi-check-circle" },
  ]);

  protected getStepColor(step: TimelineStep): string {
    return step.date ? "#22c55e" : "#d1d5db";
  }
}
