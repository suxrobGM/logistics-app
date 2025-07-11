import {CommonModule, CurrencyPipe, DatePipe} from "@angular/common";
import {Component, inject, input, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {InvoiceDto, InvoiceType} from "@/core/api/models";
import {InvoiceStatusTag} from "@/shared/components";

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.component.html",
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
  ],
})
export class LoadInvoicesListComponent {
  private readonly apiService = inject(ApiService);

  readonly loadId = input<string>();
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly isLoading = signal(true);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    //const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService.invoiceApi
      .getInvoices({
        loadId: this.loadId(),
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
