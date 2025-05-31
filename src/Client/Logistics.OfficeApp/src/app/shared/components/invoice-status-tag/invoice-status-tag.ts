import {CommonModule} from "@angular/common";
import {Component, input} from "@angular/core";
import {TagModule} from "primeng/tag";
import {InvoiceStatus, invoiceStatusOptions} from "@/core/api/models";

@Component({
  selector: "app-invoice-status-tag",
  templateUrl: "./invoice-status-tag.html",
  imports: [CommonModule, TagModule],
})
export class InvoiceStatusTag {
  public readonly status = input.required<InvoiceStatus>();

  getStatusDescription(): string {
    return (
      invoiceStatusOptions.find((option) => option.value === this.status())?.label ?? "Unknown"
    );
  }

  getStatusTagSeverity(): "success" | "warn" {
    return this.status() === InvoiceStatus.Paid ? "success" : "warn";
  }
}
