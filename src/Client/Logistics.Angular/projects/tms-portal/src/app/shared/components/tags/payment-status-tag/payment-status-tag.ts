import { Component, input } from "@angular/core";
import { Tag, TagModule } from "primeng/tag";
import { type PaymentStatus, paymentStatusOptions } from "@/core/api/models";

@Component({
  selector: "app-payment-status-tag",
  templateUrl: "./payment-status-tag.html",
  imports: [TagModule],
})
export class PaymentStatusTag {
  public readonly paymentStatus = input.required<PaymentStatus>();

  getPaymentStatusDesc(): string {
    return (
      paymentStatusOptions.find((option) => option.value === this.paymentStatus())?.label ??
      "Unknown"
    );
  }

  getPaymentStatusTagSeverity(): Tag["severity"] {
    return this.paymentStatus() === "paid" ? "success" : "warn";
  }
}
