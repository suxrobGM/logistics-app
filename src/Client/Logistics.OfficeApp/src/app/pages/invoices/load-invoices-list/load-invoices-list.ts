import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getInvoices$Json } from "@/core/api";
import { InvoiceDto, InvoiceType } from "@/core/api/models";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.html",
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
  private readonly api = inject(Api);

  protected readonly loadId = input<string>();
  protected readonly invoices = signal<InvoiceDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getInvoices$Json, {
      LoadId: this.loadId(),
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      InvoiceType: InvoiceType.Load,
    });
    if (result.success && result.data) {
      this.invoices.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }
}
