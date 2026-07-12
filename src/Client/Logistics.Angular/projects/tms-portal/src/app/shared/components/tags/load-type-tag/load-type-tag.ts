import { Component, input } from "@angular/core";
import type { LoadType } from "@logistics/shared/api";
import { Badge, UiTooltip, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

interface TypeInfo {
  label: string;
  severity: UiBadgeIntent;
  icon?: IconName;
}

const TYPE_INFO: Record<LoadType, TypeInfo> = {
  general_freight: { label: "General Freight", severity: "info", icon: "box" },
  refrigerated_goods: {
    label: "Refrigerated",
    severity: "info",
    icon: "snowflake",
  },
  hazardous_materials: {
    label: "Hazardous",
    severity: "danger",
    icon: "triangle-alert",
  },
  oversize_heavy: { label: "Oversize / Heavy", severity: "warn", icon: "truck" },
  liquid: { label: "Liquid / Tanker", severity: "info", icon: "sliders-horizontal" },
  bulk: { label: "Bulk", severity: "info", icon: "inbox" },
  vehicle: { label: "Vehicle / Car", severity: "success", icon: "car" },
  livestock: { label: "Livestock", severity: "success", icon: "paw-print" },
  intermodal_container: {
    label: "Intermodal Container",
    severity: "info",
    icon: "warehouse",
  },
  tank_container: { label: "Tank Container", severity: "info", icon: "sliders-horizontal" },
  reefer_container: { label: "Reefer Container", severity: "info", icon: "snowflake" },
  break_bulk: { label: "Break Bulk", severity: "info", icon: "inbox" },
  high_value: { label: "High Value", severity: "warn", icon: "star" },
  pharmaceutical: { label: "Pharmaceutical", severity: "info", icon: "heart" },
  project_cargo: { label: "Project Cargo", severity: "warn", icon: "briefcase" },
};

@Component({
  selector: "app-load-type-tag",
  templateUrl: "./load-type-tag.html",
  imports: [Badge, UiTooltip],
})
export class LoadTypeTag {
  /**
   * Accepts LoadType enum value or its wire string ("general_freight", etc.)
   */
  public readonly type = input.required<LoadType>();

  /** Show the icon (defaults true) */
  public readonly showIcon = input<boolean>(true);

  /** Make the tag rounded (defaults true) */
  public readonly rounded = input<boolean>(true);

  /** Optional custom tooltip. If not provided, label is used. */
  public readonly tooltip = input<string>();

  get info() {
    const normalized = this.type();
    if (normalized) {
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
