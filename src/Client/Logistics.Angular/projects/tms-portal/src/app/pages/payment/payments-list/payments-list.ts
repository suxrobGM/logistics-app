import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { type PaymentMethodType } from "@logistics/shared/api";
import { paymentMethodTypeOptions } from "@logistics/shared/api/enums";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Card, UiDataTable, UiSortHeader, UiTooltip } from "@logistics/shared/ui";
import { DataContainer, PageHeader, PaymentStatusTag } from "@/shared/components";
import { PaymentsListStore } from "../store/payments-list.store";

@Component({
  selector: "app-payments-list",
  templateUrl: "./payments-list.html",
  providers: [PaymentsListStore],
  imports: [
    AddressPipe,
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DataContainer,
    PageHeader,
    PaymentStatusTag,
    RouterModule,
    UiDataTable,
    UiSortHeader,
    UiTooltip,
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
