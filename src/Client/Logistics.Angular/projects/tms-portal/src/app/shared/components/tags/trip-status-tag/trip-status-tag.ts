import { Component, computed, input } from "@angular/core";
import type { TripStatus } from "@logistics/shared/api/models";
import { Tag } from "primeng/tag";

@Component({
  selector: "app-trip-status-tag",
  imports: [Tag],
  templateUrl: "./trip-status-tag.html",
})
export class TripStatusTag {
  /** Accepts enum or raw string ('draft', 'in_transit', etc.) */
  public readonly status = input.required<TripStatus | string>();

  public readonly rounded = input<boolean>(true);
  public readonly showIcon = input<boolean>(true);

  private readonly normalized = computed(() => ("" + this.status()).toLowerCase() as TripStatus);

  protected readonly label = computed(() => {
    switch (this.normalized()) {
      case "draft":
        return "Draft";
      case "dispatched":
        return "Dispatched";
      case "in_transit":
        return "In Transit";
      case "cancelled":
        return "Cancelled";
      case "completed":
        return "Completed";
      default:
        return this.status() as string;
    }
  });

  protected readonly severity = computed<Tag["severity"]>(() => {
    switch (this.normalized()) {
      case "cancelled":
        return "danger";
      case "completed":
        return "success";
      case "in_transit":
        return "warn";
      case "dispatched":
        return "info";
      case "draft":
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
      case "draft":
        return "pi pi-pencil";
      case "dispatched":
        return "pi pi-send";
      case "in_transit":
        return "pi pi-truck";
      case "cancelled":
        return "pi pi-times";
      case "completed":
        return "pi pi-check";
      default:
        return undefined;
    }
  });
}
