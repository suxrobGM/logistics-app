import { Component, computed, input } from "@angular/core";
import { type EmployeeStatus } from "@logistics/shared/api";
import { employeeStatusOptions } from "@logistics/shared/api/enums";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-employee-status-tag",
  templateUrl: "./employee-status-tag.html",
  imports: [TagModule],
})
export class EmployeeStatusTag {
  public readonly status = input.required<EmployeeStatus>();

  protected readonly label = computed(() => {
    return employeeStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  });

  protected readonly severity = computed((): Tag["severity"] => {
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

  protected readonly icon = computed((): string => {
    switch (this.status()) {
      case "active":
        return "pi pi-check-circle";
      case "on_leave":
        return "pi pi-calendar";
      case "suspended":
        return "pi pi-pause-circle";
      case "terminated":
        return "pi pi-times-circle";
      default:
        return "pi pi-circle";
    }
  });
}
