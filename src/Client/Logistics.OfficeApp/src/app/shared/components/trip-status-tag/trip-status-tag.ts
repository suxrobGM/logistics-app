import {Component, computed, input} from "@angular/core";
import {Tag} from "primeng/tag";
import {TripStatus} from "@/core/api/models";

@Component({
  selector: "app-trip-status-tag",
  imports: [Tag],
  templateUrl: "./trip-status-tag.html",
})
export class TripStatusTag {
  /** Accepts enum or raw string ('planned', 'in_transit', etc.) */
  public readonly status = input.required<TripStatus | string>();

  public readonly rounded = input<boolean>(true);
  public readonly showIcon = input<boolean>(true);

  private readonly normalized = computed(() => ("" + this.status()).toLowerCase());

  protected readonly label = computed(() => {
    switch (this.normalized()) {
      case TripStatus.Planned:
        return "Planned";
      case TripStatus.Dispatched:
        return "Dispatched";
      case TripStatus.InTransit:
        return "In Transit";
      case TripStatus.Cancelled:
        return "Cancelled";
      case TripStatus.Completed:
        return "Completed";
      default:
        return this.status() as string;
    }
  });

  protected readonly severity = computed<string>(() => {
    switch (this.normalized()) {
      case TripStatus.Cancelled:
        return "danger";
      case TripStatus.Completed:
        return "success";
      case TripStatus.InTransit:
        return "warning";
      case TripStatus.Dispatched:
        return "info";
      case TripStatus.Planned:
        return "info";
      default:
        return "info";
    }
  });

  protected readonly icon = computed<string | undefined>(() => {
    if (!this.showIcon()) {
      return undefined;
    }
    switch (this.normalized()) {
      case TripStatus.Planned:
        return "pi pi-calendar";
      case TripStatus.Dispatched:
        return "pi pi-send";
      case TripStatus.InTransit:
        return "pi pi-truck";
      case TripStatus.Cancelled:
        return "pi pi-times";
      case TripStatus.Completed:
        return "pi pi-check";
      default:
        return undefined;
    }
  });
}
