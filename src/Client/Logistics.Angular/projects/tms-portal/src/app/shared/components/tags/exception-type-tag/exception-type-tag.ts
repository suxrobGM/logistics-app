import { Component, input } from "@angular/core";
import type { LoadExceptionType } from "@logistics/shared/api";
import { loadExceptionTypeOptions } from "@logistics/shared/api/enums";
import { Badge, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-exception-type-tag",
  templateUrl: "./exception-type-tag.html",
  imports: [Badge],
})
export class ExceptionTypeTag {
  public readonly type = input.required<LoadExceptionType>();

  getExceptionTypeDesc(): string {
    return loadExceptionTypeOptions.find((option) => option.value === this.type())?.label ?? "";
  }

  getExceptionTypeSeverity(): UiBadgeIntent {
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

  getExceptionTypeIcon(): IconName {
    switch (this.type()) {
      case "accident":
        return "triangle-alert";
      case "delay":
        return "clock";
      case "weather_delay":
        return "cloud";
      case "mechanical_failure":
        return "wrench";
      case "route_change":
        return "navigation";
      case "customer_request":
        return "user";
      case "other":
      default:
        return "info";
    }
  }
}
