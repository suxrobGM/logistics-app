import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import type { DefectSeverity, DvirDefectDto } from "@logistics/shared/api";
import { TagModule } from "primeng/tag";
import type { TagSeverity } from "@/shared/types";

@Component({
  selector: "app-dvir-defects-list",
  templateUrl: "./dvir-defects-list.html",
  imports: [DatePipe, TagModule],
})
export class DvirDefectsList {
  public readonly defects = input.required<DvirDefectDto[]>();

  protected getSeverityColor(severity: DefectSeverity | undefined): TagSeverity {
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
