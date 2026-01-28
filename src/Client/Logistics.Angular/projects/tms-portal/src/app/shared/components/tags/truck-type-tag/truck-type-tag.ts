import { Component, input } from "@angular/core";
import type { TruckType } from "@logistics/shared/api";
import { Tag } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";

interface TypeInfo {
  label: string;
  severity: Tag["severity"];
  icon?: string;
}

const TYPE_INFO: Record<TruckType, TypeInfo> = {
  flatbed: { label: "Flatbed", severity: "secondary", icon: "pi pi-minus" },
  freight_truck: { label: "Freight", severity: "info", icon: "pi pi-truck" },
  reefer: { label: "Reefer", severity: "info", icon: "pi pi-snowflake" },
  tanker: { label: "Tanker", severity: "contrast", icon: "pi pi-circle" },
  box_truck: { label: "Box Truck", severity: "info", icon: "pi pi-box" },
  dump_truck: { label: "Dump Truck", severity: "warn", icon: "pi pi-sort-up" },
  tow_truck: { label: "Tow Truck", severity: "secondary", icon: "pi pi-link" },
  car_hauler: { label: "Car Hauler", severity: "success", icon: "pi pi-car" },
};

@Component({
  selector: "app-truck-type-tag",
  templateUrl: "./truck-type-tag.html",
  imports: [Tag, TooltipModule],
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
