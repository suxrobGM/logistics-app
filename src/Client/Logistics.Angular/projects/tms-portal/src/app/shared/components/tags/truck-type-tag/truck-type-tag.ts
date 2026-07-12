import { Component, input } from "@angular/core";
import type { TruckType } from "@logistics/shared/api";
import { Badge, UiTooltip, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

interface TypeInfo {
  label: string;
  severity: UiBadgeIntent;
  icon?: IconName;
}

const TYPE_INFO: Record<TruckType, TypeInfo> = {
  flatbed: { label: "Flatbed", severity: "secondary", icon: "minus" },
  freight_truck: { label: "Freight", severity: "info", icon: "truck" },
  reefer: { label: "Reefer", severity: "info", icon: "snowflake" },
  tanker: { label: "Tanker", severity: "contrast", icon: "circle" },
  box_truck: { label: "Box Truck", severity: "info", icon: "box" },
  dump_truck: { label: "Dump Truck", severity: "warn", icon: "arrow-up" },
  tow_truck: { label: "Tow Truck", severity: "secondary", icon: "link" },
  car_hauler: { label: "Car Hauler", severity: "success", icon: "car" },
  container_truck: { label: "Container Truck", severity: "info", icon: "warehouse" },
  tautliner: { label: "Tautliner", severity: "secondary", icon: "truck" },
  low_loader: { label: "Low Loader", severity: "warn", icon: "arrow-down" },
  car_transporter: { label: "Car Transporter", severity: "success", icon: "car" },
  swap_body: { label: "Swap Body", severity: "info", icon: "refresh-cw" },
  curtainsider: { label: "Curtainsider", severity: "info", icon: "truck" },
};

@Component({
  selector: "app-truck-type-tag",
  templateUrl: "./truck-type-tag.html",
  imports: [Badge, UiTooltip],
})
export class TruckTypeTag {
  public readonly type = input.required<TruckType>();
  public readonly showIcon = input<boolean>(true);
  public readonly rounded = input<boolean>(true);
  public readonly tooltip = input<string>();

  get info() {
    const normalized = this.type();
    if (normalized && TYPE_INFO[normalized]) {
      return TYPE_INFO[normalized];
    }

    // fallback for unknown values
    const v = typeof this.type() === "string" ? this.type() : String(this.type());
    return { label: this.titleCase(v.replace(/_/g, " ")), severity: "secondary" } as TypeInfo;
  }

  private titleCase(s: string) {
    return s.replace(/\w\S*/g, (w) => w[0].toUpperCase() + w.slice(1).toLowerCase());
  }
}
