import { Component, computed, input } from "@angular/core";
import { type EmployeeStatus } from "@logistics/shared/api";
import { employeeStatusOptions } from "@logistics/shared/api/enums";
import { Badge, type IconName, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-employee-status-tag",
  templateUrl: "./employee-status-tag.html",
  imports: [Badge],
})
export class EmployeeStatusTag {
  public readonly status = input.required<EmployeeStatus>();

  protected readonly label = computed(() => {
    return employeeStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  });

  protected readonly severity = computed((): UiBadgeIntent => {
    switch (this.status()) {
      case "active":
        return "success";
      case "on_leave":
        return "warn";
      case "suspended":
        return "warn";
      case "terminated":
        return "danger";
      default:
        return "info";
    }
  });

  protected readonly icon = computed((): IconName => {
    switch (this.status()) {
      case "active":
        return "circle-check";
      case "on_leave":
        return "calendar";
      case "suspended":
        return "pause-circle";
      case "terminated":
        return "circle-x";
      default:
        return "circle";
    }
  });
}
