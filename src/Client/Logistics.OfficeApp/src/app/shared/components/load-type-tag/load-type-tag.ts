import {Component, input} from "@angular/core";
import {Tag} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {LoadType} from "@/core/api/models";

interface TypeInfo {
  label: string;
  severity: string;
  icon?: string;
}

const TYPE_INFO: Record<LoadType, TypeInfo> = {
  [LoadType.GeneralFreight]: {label: "General Freight", severity: "info", icon: "pi pi-box"},
  [LoadType.RefrigeratedGoods]: {
    label: "Refrigerated",
    severity: "primary",
    icon: "pi pi-snowflake",
  },
  [LoadType.HazardousMaterials]: {
    label: "Hazardous",
    severity: "danger",
    icon: "pi pi-exclamation-triangle",
  },
  [LoadType.OversizeHeavy]: {label: "Oversize / Heavy", severity: "warning", icon: "pi pi-truck"},
  [LoadType.Liquid]: {label: "Liquid / Tanker", severity: "primary", icon: "pi pi-sliders-h"},
  [LoadType.Bulk]: {label: "Bulk", severity: "secondary", icon: "pi pi-inbox"},
  [LoadType.Vehicle]: {label: "Vehicle / Car", severity: "success", icon: "pi pi-car"},
  [LoadType.Livestock]: {label: "Livestock", severity: "success", icon: "pi pi-paw"},
};

@Component({
  selector: "app-load-type-tag",
  templateUrl: "./load-type-tag.html",
  imports: [Tag, TooltipModule],
})
export class LoadTypeTag {
  /**
   * Accepts LoadType enum value or its wire string ("general_freight", etc.)
   */
  public readonly type = input.required<LoadType | string>();

  /** Show the icon (defaults true) */
  public readonly showIcon = input<boolean>(true);

  /** Make the tag rounded (defaults true) */
  public readonly rounded = input<boolean>(true);

  /** Optional custom tooltip. If not provided, label is used. */
  public readonly tooltip = input<string>();

  get info() {
    const normalized = this.normalize(this.type());
    if (normalized) {
      return TYPE_INFO[normalized];
    }

    // fallback for unknown values
    const v = typeof this.type() === "string" ? this.type() : String(this.type());
    return {label: this.titleCase(v.replace(/_/g, " ")), severity: "secondary"};
  }

  private normalize(v: LoadType | string | undefined | null): LoadType | null {
    if (!v) {
      return null;
    }

    // If already in enum (ts enum value is string union), return as-is
    if (Object.values(LoadType).includes(v as LoadType)) return v as LoadType;

    // Try to map string to enum value (supports both "Vehicle" and "vehicle" / "vehicle_car")
    const s = String(v).toLowerCase();
    const match = (Object.values(LoadType) as string[]).find((x) => x.toLowerCase() === s);
    return (match as LoadType) ?? null;
  }

  private titleCase(s: string) {
    return s.replace(/\w\S*/g, (w) => w[0].toUpperCase() + w.slice(1).toLowerCase());
  }
}
