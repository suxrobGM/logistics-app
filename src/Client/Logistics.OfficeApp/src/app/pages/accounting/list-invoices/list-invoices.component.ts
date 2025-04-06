import {CommonModule, CurrencyPipe, DatePipe} from "@angular/common";
import {Component} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {PaymentStatus} from "@/core/enums";
import {InvoiceDto} from "@/core/models";
import {PredefinedDateRanges} from "@/core/utils";

@Component({
  selector: "app-list-invoices",
  standalone: true,
  templateUrl: "./list-invoices.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    PaymentStatusTagComponent,
  ],
})
export class ListInvoicesComponent {
  public paymentStatus = PaymentStatus;
  public invoices: InvoiceDto[] = [];
  public isLoading = true;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {}

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService
      .getInvoices({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        startDate: past90days.startDate,
        endDate: past90days.endDate,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.invoices = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
      });
  }
}
