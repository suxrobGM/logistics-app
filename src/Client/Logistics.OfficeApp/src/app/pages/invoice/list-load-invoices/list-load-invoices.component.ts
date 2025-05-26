import {CommonModule, CurrencyPipe, DatePipe} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {InvoiceStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {InvoiceDto, InvoiceType} from "@/core/api/models";

@Component({
  selector: "app-list-load-invoices",
  standalone: true,
  templateUrl: "./list-load-invoices.component.html",
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
    InvoiceStatusTagComponent,
  ],
})
export class ListLoadInvoicesComponent {
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly isLoading = signal(true);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

  load(event: TableLazyLoadEvent) {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    //const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService.invoiceApi
      .getInvoices({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        invoiceType: InvoiceType.Load,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.invoices.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }
}
