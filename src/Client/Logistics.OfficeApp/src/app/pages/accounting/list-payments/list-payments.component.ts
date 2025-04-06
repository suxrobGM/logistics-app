import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {PaymentFor, PaymentForEnum, PaymentMethod, PaymentMethodEnum} from "@/core/enums";
import {PaymentDto} from "@/core/models";
import {AddressPipe} from "@/core/pipes";
import {PredefinedDateRanges} from "@/core/utils";

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
  public payments: PaymentDto[] = [];
  public isLoading = false;
  public totalRecords = 0;
  public first = 0;

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

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService
      .getPayments({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        startDate: past90days.startDate,
        endDate: past90days.endDate,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.payments = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
      });
  }

  getPaymentMethodDesc(enumValue?: PaymentMethod): string {
    if (enumValue == null) {
      return "N/A";
    }

    return PaymentMethodEnum.getValue(enumValue).description;
  }

  getPaymentForDesc(enumValue: PaymentFor): string {
    return PaymentForEnum.getValue(enumValue).description;
  }
}
