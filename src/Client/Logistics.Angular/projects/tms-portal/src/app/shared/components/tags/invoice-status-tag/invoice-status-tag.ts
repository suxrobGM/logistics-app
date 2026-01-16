import { Component, input } from "@angular/core";
import { type InvoiceStatus, invoiceStatusOptions } from "@logistics/shared/api/models";
import { Tag, TagModule } from "primeng/tag";

@Component({
  selector: "app-invoice-status-tag",
  templateUrl: "./invoice-status-tag.html",
  imports: [TagModule],
})
export class InvoiceStatusTag {
  public readonly status = input.required<InvoiceStatus>();

  getStatusDescription(): string {
    return (
      invoiceStatusOptions.find((option) => option.value === this.status())?.label ?? "Unknown"
    );
  }

  getStatusTagSeverity(): Tag["severity"] {
    return this.status() === "paid" ? "success" : "warn";
  }
}
