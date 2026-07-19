import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import type { DefectSeverity, DvirDefectDto } from "@logistics/shared/api";
import { Badge, Icon, Stack, Surface, Typography, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-dvir-defects-list",
  templateUrl: "./dvir-defects-list.html",
  imports: [Badge, DatePipe, Icon, Stack, Surface, Typography],
})
export class DvirDefectsList {
  public readonly defects = input.required<DvirDefectDto[]>();

  protected getSeverityColor(severity: DefectSeverity | undefined): UiBadgeIntent {
    switch (severity) {
      case "minor":
        return "info";
      case "major":
        return "warn";
      case "out_of_service":
        return "danger";
      default:
        return "secondary";
    }
  }
}
