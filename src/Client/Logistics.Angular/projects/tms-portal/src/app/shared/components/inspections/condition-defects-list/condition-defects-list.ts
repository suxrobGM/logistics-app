import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import type { ConditionDefectDto } from "@logistics/shared/api";
import { Badge, UiDataTable, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-condition-defects-list",
  templateUrl: "./condition-defects-list.html",
  imports: [Badge, CommonModule, UiDataTable],
})
export class ConditionDefectsList {
  public readonly defects = input<ConditionDefectDto[]>([]);

  getSeverityBadge(severity?: string): UiBadgeIntent {
    switch (severity) {
      case "out_of_service":
        return "danger";
      case "major":
        return "warn";
      case "minor":
      default:
        return "info";
    }
  }
}
