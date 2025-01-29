import {Component, Input} from "@angular/core";
import {CommonModule} from "@angular/common";
import {PaymentStatus, PaymentStatusEnum} from "@/core/enums";
import {TagModule} from "primeng/tag";

@Component({
  selector: "app-payment-status-tag",
  standalone: true,
  templateUrl: "./payment-status-tag.component.html",
  imports: [CommonModule, TagModule],
})
export class PaymentStatusTagComponent {
  @Input({required: true}) paymentStatus!: PaymentStatus;

  getPaymentStatusDesc(enumValue: PaymentStatus): string {
    return PaymentStatusEnum.getValue(enumValue).description;
  }

  getPaymentStatusTagSeverity(paymentStatus: PaymentStatus): "success" | "warn" {
    return paymentStatus === PaymentStatus.Paid ? "success" : "warn";
  }
}
