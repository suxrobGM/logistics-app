import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {PaymentDto, PaymentMethodType, paymentMethodTypeOptions} from "@/core/api/models";
import {AddressPipe} from "@/core/pipes";
import {PredefinedDateRanges} from "@/core/utilities";

@Component({
  selector: "app-list-payments",
  standalone: true,
  templateUrl: "./list-payments.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    TooltipModule,
    RouterModule,
    ButtonModule,
    PaymentStatusTagComponent,
    AddressPipe,
  ],
})
export class ListPaymentsComponent {
  readonly payments = signal<PaymentDto[]>([]);
  readonly isLoading = signal(false);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

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
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
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
