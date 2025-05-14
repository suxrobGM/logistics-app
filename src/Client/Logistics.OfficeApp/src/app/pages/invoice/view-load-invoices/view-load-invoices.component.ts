import {CommonModule} from "@angular/common";
import {Component, input, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {InvoiceStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {InvoiceDto} from "@/core/api/models";

@Component({
  selector: "app-view-load-invoices",
  standalone: true,
  templateUrl: "./view-load-invoices.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    InvoiceStatusTagComponent,
    RouterModule,
  ],
})
export class ViewLoadInvoicesComponent {
  readonly loadId = input.required<string>();
  readonly isLoading = signal(false);
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.invoiceApi
      .getInvoices({
        loadId: this.loadId()!,
        orderBy: sortField,
        page: page,
        pageSize: rows,
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
