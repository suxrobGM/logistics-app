import { Component, input } from "@angular/core";
import { Tag } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import type { LoadType } from "@/core/api/models";

interface TypeInfo {
  label: string;
  severity: Tag["severity"];
  icon?: string;
}

const TYPE_INFO: Record<LoadType, TypeInfo> = {
  "general_freight": { label: "General Freight", severity: "info", icon: "pi pi-box" },
  "refrigerated_goods": {
    label: "Refrigerated",
    severity: "info",
    icon: "pi pi-snowflake",
  },
  "hazardous_materials": {
    label: "Hazardous",
    severity: "danger",
    icon: "pi pi-exclamation-triangle",
  },
  "oversize_heavy": { label: "Oversize / Heavy", severity: "warn", icon: "pi pi-truck" },
  "liquid": { label: "Liquid / Tanker", severity: "info", icon: "pi pi-sliders-h" },
  "bulk": { label: "Bulk", severity: "info", icon: "pi pi-inbox" },
  "vehicle": { label: "Vehicle / Car", severity: "success", icon: "pi pi-car" },
  "livestock": { label: "Livestock", severity: "success", icon: "pi pi-paw" },
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
