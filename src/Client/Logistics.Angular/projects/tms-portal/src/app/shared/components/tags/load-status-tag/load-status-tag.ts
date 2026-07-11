import { Component, input } from "@angular/core";
import { type LoadStatus } from "@logistics/shared/api";
import { loadStatusOptions } from "@logistics/shared/api/enums";
import { Badge, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-load-status-tag",
  templateUrl: "./load-status-tag.html",
  imports: [Badge],
})
export class LoadStatusTag {
  public readonly status = input.required<LoadStatus>();

  getLoadStatusDesc(): string {
    return loadStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  }

  getLoadStatusSeverity(): UiBadgeIntent {
    return this.status() === "delivered" ? "success" : "info";
  }

  getLoadStatusIcon(): IconName {
    return this.status() === "delivered" ? "check" : "truck";
  }
}
