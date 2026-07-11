import { Component, input } from "@angular/core";
import type { ExpenseStatus } from "@logistics/shared/api";
import { Badge, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-expense-status-tag",
  templateUrl: "./expense-status-tag.html",
  imports: [Badge],
})
export class ExpenseStatusTag {
  public readonly status = input.required<ExpenseStatus | undefined>();

  getLabel(): string {
    switch (this.status()) {
      case "pending":
        return "Pending";
      case "approved":
        return "Approved";
      case "rejected":
        return "Rejected";
      case "paid":
        return "Paid";
      default:
        return "Unknown";
    }
  }

  getSeverity(): UiBadgeIntent {
    switch (this.status()) {
      case "pending":
        return "warn";
      case "approved":
        return "success";
      case "rejected":
        return "danger";
      case "paid":
        return "info";
      default:
        return "secondary";
    }
  }
}
