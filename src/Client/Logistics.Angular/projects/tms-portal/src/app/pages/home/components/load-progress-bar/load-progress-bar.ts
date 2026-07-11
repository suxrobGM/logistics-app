import { Component, computed, input } from "@angular/core";
import type { LoadStatus } from "@logistics/shared/api";
import { UiTooltip } from "@logistics/shared/ui";

@Component({
  selector: "app-load-progress-bar",
  templateUrl: "./load-progress-bar.html",
  imports: [UiTooltip],
})
export class LoadProgressBarComponent {
  readonly status = input.required<LoadStatus | undefined>();
  readonly originCity = input<string>();
  readonly destinationCity = input<string>();
  readonly compact = input(false);

  protected readonly progress = computed(() => {
    switch (this.status()) {
      case "draft":
        return 0;
      case "dispatched":
        return 33;
      case "picked_up":
        return 66;
      case "delivered":
        return 100;
      case "cancelled":
        return 0;
      default:
        return 0;
    }
  });

  protected readonly statusLabel = computed(() => {
    switch (this.status()) {
      case "draft":
        return "Draft";
      case "dispatched":
        return "En route to pickup";
      case "picked_up":
        return "In transit";
      case "delivered":
        return "Delivered";
      case "cancelled":
        return "Cancelled";
      default:
        return "Unknown";
    }
  });
}
