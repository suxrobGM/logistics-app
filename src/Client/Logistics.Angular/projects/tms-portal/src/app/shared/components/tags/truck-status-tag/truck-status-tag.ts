import { Component, computed, input } from "@angular/core";
import { type TruckStatus } from "@logistics/shared/api";
import { truckStatusOptions } from "@logistics/shared/api/enums";
import { Badge, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-truck-status-tag",
  templateUrl: "./truck-status-tag.html",
  imports: [Badge],
})
export class TruckStatusTag {
  public readonly status = input.required<TruckStatus>();

  protected readonly label = computed(() => {
    return truckStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  });

  protected readonly severity = computed((): UiBadgeIntent => {
    switch (this.status()) {
      case "available":
        return "success";
      case "en_route":
        return "info";
      case "loading":
      case "unloading":
        return "warn";
      case "maintenance":
        return "warn";
      case "out_of_service":
        return "danger";
      case "offline":
        return "secondary";
      default:
        return "info";
    }
  });

  protected readonly icon = computed((): IconName => {
    switch (this.status()) {
      case "available":
        return "circle-check";
      case "en_route":
        return "truck";
      case "loading":
        return "arrow-down";
      case "unloading":
        return "arrow-up";
      case "maintenance":
        return "wrench";
      case "out_of_service":
        return "ban";
      case "offline":
        return "wifi-off";
      default:
        return "circle";
    }
  });
}
