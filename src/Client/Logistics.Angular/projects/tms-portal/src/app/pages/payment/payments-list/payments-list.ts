import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { type PaymentMethodType, paymentMethodTypeOptions } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PaymentStatusTag } from "@/shared/components";
import { PaymentsListStore } from "../store/payments-list.store";

@Component({
  selector: "app-payments-list",
  templateUrl: "./payments-list.html",
  providers: [PaymentsListStore],
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    TooltipModule,
    RouterModule,
    ButtonModule,
    PaymentStatusTag,
    AddressPipe,
    DataContainer,
  ],
})
export class PaymentsListComponent {
  protected readonly store = inject(PaymentsListStore);

  getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }

    return (
      paymentMethodTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown"
    );
  }
}
