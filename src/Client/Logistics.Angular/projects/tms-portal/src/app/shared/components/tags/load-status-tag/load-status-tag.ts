import { Component, input } from "@angular/core";
import { type LoadStatus, loadStatusOptions } from "@logistics/shared/api/models";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-load-status-tag",
  templateUrl: "./load-status-tag.html",
  imports: [TagModule],
})
export class LoadStatusTag {
  public readonly status = input.required<LoadStatus>();

  getLoadStatusDesc(): string {
    return loadStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  }

  getLoadStatusSeverity(): Tag["severity"] {
    return this.status() === "delivered" ? "success" : "info";
  }

  getLoadStatusIcon(): string {
    return this.status() === "delivered" ? "pi pi-check" : "pi pi-truck";
  }
}
