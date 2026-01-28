import { Component, computed, input } from "@angular/core";
import { type TruckStatus } from "@logistics/shared/api";
import { truckStatusOptions } from "@logistics/shared/api/enums";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-truck-status-tag",
  templateUrl: "./truck-status-tag.html",
  imports: [TagModule],
})
export class TruckStatusTag {
  public readonly status = input.required<TruckStatus>();

  protected readonly label = computed(() => {
    return truckStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  });

  protected readonly severity = computed((): Tag["severity"] => {
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

  protected readonly icon = computed((): string => {
    switch (this.status()) {
      case "available":
        return "pi pi-check-circle";
      case "en_route":
        return "pi pi-truck";
      case "loading":
        return "pi pi-arrow-down";
      case "unloading":
        return "pi pi-arrow-up";
      case "maintenance":
        return "pi pi-wrench";
      case "out_of_service":
        return "pi pi-ban";
      case "offline":
        return "pi pi-wifi-off";
      default:
        return "pi pi-circle";
    }
  });
}
