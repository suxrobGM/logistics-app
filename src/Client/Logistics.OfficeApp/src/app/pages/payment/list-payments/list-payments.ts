import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {PaymentDto, PaymentMethodType, paymentMethodTypeOptions} from "@/core/api/models";
import {PaymentStatusTag} from "@/shared/components";
import {AddressPipe} from "@/shared/pipes";
import {PredefinedDateRanges} from "@/shared/utils";

@Component({
  selector: "app-list-payments",
  templateUrl: "./list-payments.html",
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
export class ListPaymentsComponent {
  private readonly apiService = inject(ApiService);

  protected readonly payments = signal<PaymentDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  // search(event: Event) {
  //   this.isLoading = true;
  //   const searchValue = (event.target as HTMLInputElement).value;

  //   this.apiService.getPayments({search: searchValue}).subscribe((result) => {
  //     if (result.isSuccess && result.data) {
  //       this.payments = result.data;
  //       this.totalRecords = result.totalItems;
  //     }

  //     this.isLoading = false;
  //   });
  // }

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.formatSortField(event.sortField as string, event.sortOrder);
    const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService.paymentApi
      .getPayments({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        startDate: past90days.startDate,
        endDate: past90days.endDate,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.payments.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
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
