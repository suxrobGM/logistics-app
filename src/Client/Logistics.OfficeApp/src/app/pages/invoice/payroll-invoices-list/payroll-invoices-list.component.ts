import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {InvoiceStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {InvoiceDto, InvoiceType, SalaryType, salaryTypeOptions} from "@/core/api/models";

@Component({
  selector: "app-payroll-invoices-list",
  templateUrl: "./payroll-invoices-list.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    InputTextModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTagComponent,
    IconFieldModule,
    InputIconModule,
  ],
})
export class PayrollInvoicesListComponent {
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly isLoading = signal(false);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

  search(event: Event): void {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.invoiceApi.getInvoices({employeeName: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.invoices.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.invoiceApi
      .getInvoices({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        invoiceType: InvoiceType.Payroll,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.invoices.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }

  // getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
  //   if (enumValue == null) {
  //     return "N/A";
  //   }

  //   return paymentMethodTypeOptions.find((x) => x.value === enumValue)?.label ?? "N/A";
  // }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
