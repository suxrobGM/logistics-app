import { Component, input } from "@angular/core";
import { type InvoiceStatus } from "@logistics/shared/api";
import { invoiceStatusOptions } from "@logistics/shared/api/enums";
import { Badge, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-invoice-status-tag",
  templateUrl: "./invoice-status-tag.html",
  imports: [Badge],
})
export class InvoiceStatusTag {
  public readonly status = input.required<InvoiceStatus>();

  getStatusDescription(): string {
    return (
      invoiceStatusOptions.find((option) => option.value === this.status())?.label ?? "Unknown"
    );
  }

  getStatusTagSeverity(): UiBadgeIntent {
    switch (this.status()) {
      case "paid":
      case "approved":
        return "success";
      case "rejected":
      case "cancelled":
        return "danger";
      case "pending_approval":
        return "info";
      default:
        return "warn";
    }
  }
}
