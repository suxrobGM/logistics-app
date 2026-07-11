import { Component, input } from "@angular/core";
import { type PaymentStatus } from "@logistics/shared/api";
import { paymentStatusOptions } from "@logistics/shared/api/enums";
import { Badge, type UiBadgeIntent } from "@logistics/shared/ui";

@Component({
  selector: "app-payment-status-tag",
  templateUrl: "./payment-status-tag.html",
  imports: [Badge],
})
export class PaymentStatusTag {
  public readonly paymentStatus = input.required<PaymentStatus>();

  getPaymentStatusDesc(): string {
    return (
      paymentStatusOptions.find((option) => option.value === this.paymentStatus())?.label ??
      "Unknown"
    );
  }

  getPaymentStatusTagSeverity(): UiBadgeIntent {
    return this.paymentStatus() === "paid" ? "success" : "warn";
  }
}
