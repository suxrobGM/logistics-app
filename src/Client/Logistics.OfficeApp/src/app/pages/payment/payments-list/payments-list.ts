import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getPayments$Json } from "@/core/api";
import { PaymentDto, PaymentMethodType, paymentMethodTypeOptions } from "@/core/api/models";
import { PaymentStatusTag } from "@/shared/components";
import { AddressPipe } from "@/shared/pipes";
import { PredefinedDateRanges } from "@/shared/utils";

@Component({
  selector: "app-payments-list",
  templateUrl: "./payments-list.html",
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    TooltipModule,
    RouterModule,
    ButtonModule,
    PaymentStatusTag,
    AddressPipe,
  ],
})
export class PaymentsListComponent {
  private readonly api = inject(Api);

  protected readonly payments = signal<PaymentDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);
    const past90days = PredefinedDateRanges.getPast90Days();

    const result = await this.api.invoke(getPayments$Json, {
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      StartDate: past90days.startDate.toISOString(),
      EndDate: past90days.endDate.toISOString(),
    });

    if (result.success && result.data) {
      this.payments.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }

    return (
      paymentMethodTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown"
    );
  }
}
