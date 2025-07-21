import {CommonModule} from "@angular/common";
import {Component, input} from "@angular/core";
import {TagModule} from "primeng/tag";
import {PaymentStatus, paymentStatusOptions} from "@/core/api/models";

@Component({
  selector: "app-payment-status-tag",
  templateUrl: "./payment-status-tag.html",
  imports: [CommonModule, TagModule],
})
export class PaymentStatusTag {
  public readonly paymentStatus = input.required<PaymentStatus>();

  getPaymentStatusDesc(): string {
    return (
      paymentStatusOptions.find((option) => option.value === this.paymentStatus())?.label ??
      "Unknown"
    );
  }

  getPaymentStatusTagSeverity(): "success" | "warn" {
    return this.paymentStatus() === PaymentStatus.Paid ? "success" : "warn";
  }
}
