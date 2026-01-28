import { Component, computed, input } from "@angular/core";
import { type CustomerStatus } from "@logistics/shared/api";
import { customerStatusOptions } from "@logistics/shared/api/enums";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-customer-status-tag",
  templateUrl: "./customer-status-tag.html",
  imports: [TagModule],
})
export class CustomerStatusTag {
  public readonly status = input.required<CustomerStatus>();

  protected readonly label = computed(() => {
    return customerStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  });

  protected readonly severity = computed((): Tag["severity"] => {
    switch (this.status()) {
      case "active":
        return "success";
      case "inactive":
        return "secondary";
      case "prospect":
        return "info";
      default:
        return "info";
    }
  });

  protected readonly icon = computed((): string => {
    switch (this.status()) {
      case "active":
        return "pi pi-check-circle";
      case "inactive":
        return "pi pi-minus-circle";
      case "prospect":
        return "pi pi-star";
      default:
        return "pi pi-circle";
    }
  });
}
