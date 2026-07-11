import { Component, input } from "@angular/core";
import type { ExpenseType } from "@logistics/shared/api";
import { Badge, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-expense-type-tag",
  templateUrl: "./expense-type-tag.html",
  imports: [Badge],
})
export class ExpenseTypeTag {
  public readonly type = input.required<ExpenseType | undefined>();

  getLabel(): string {
    switch (this.type()) {
      case "company":
        return "Company";
      case "truck":
        return "Truck";
      case "body_shop":
        return "Body Shop";
      default:
        return "Unknown";
    }
  }

  getSeverity(): UiBadgeIntent {
    switch (this.type()) {
      case "company":
        return "info";
      case "truck":
        return "success";
      case "body_shop":
        return "warn";
      default:
        return "secondary";
    }
  }
}
