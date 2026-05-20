import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import type { ConditionDefectDto } from "@logistics/shared/api";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-condition-defects-list",
  templateUrl: "./condition-defects-list.html",
  imports: [CommonModule, TableModule, TagModule],
})
export class ConditionDefectsList {
  public readonly defects = input<ConditionDefectDto[]>([]);

  getSeverityBadge(severity?: string): "danger" | "warn" | "info" {
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
