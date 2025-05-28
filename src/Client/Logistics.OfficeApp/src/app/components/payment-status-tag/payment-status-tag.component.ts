import {CommonModule} from "@angular/common";
import {Component, input} from "@angular/core";
import {TagModule} from "primeng/tag";
import {PaymentStatus, paymentStatusOptions} from "@/core/api/models";

@Component({
  selector: "app-payment-status-tag",
  templateUrl: "./payment-status-tag.component.html",
  imports: [CommonModule, TagModule],
})
export class PaymentStatusTagComponent {
  public readonly paymentStatus = input.required<PaymentStatus>();

  getPaymentStatusDesc(enumValue: PaymentStatus): string {
    return paymentStatusOptions.find((option) => option.value === enumValue)?.label ?? "Unknown";
  }

  getPaymentStatusTagSeverity(paymentStatus: PaymentStatus): "success" | "warn" {
    return paymentStatus === PaymentStatus.Paid ? "success" : "warn";
  }
}
