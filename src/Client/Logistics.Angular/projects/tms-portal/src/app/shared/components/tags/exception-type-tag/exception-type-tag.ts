import { Component, input } from "@angular/core";
import type { LoadExceptionType } from "@logistics/shared/api";
import { loadExceptionTypeOptions } from "@logistics/shared/api/enums";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-exception-type-tag",
  templateUrl: "./exception-type-tag.html",
  imports: [TagModule],
})
export class ExceptionTypeTag {
  public readonly type = input.required<LoadExceptionType>();

  getExceptionTypeDesc(): string {
    return loadExceptionTypeOptions.find((option) => option.value === this.type())?.label ?? "";
  }

  getExceptionTypeSeverity(): Tag["severity"] {
    switch (this.type()) {
      case "accident":
        return "danger";
      case "delay":
      case "mechanical_failure":
        return "warn";
      case "weather_delay":
        return "info";
      case "customer_request":
        return "success";
      case "route_change":
        return "contrast";
      case "other":
      default:
        return "secondary";
    }
  }

  getExceptionTypeIcon(): string {
    switch (this.type()) {
      case "accident":
        return "pi pi-exclamation-triangle";
      case "delay":
        return "pi pi-clock";
      case "weather_delay":
        return "pi pi-cloud";
      case "mechanical_failure":
        return "pi pi-wrench";
      case "route_change":
        return "pi pi-directions";
      case "customer_request":
        return "pi pi-user";
      case "other":
      default:
        return "pi pi-info-circle";
    }
  }
}
