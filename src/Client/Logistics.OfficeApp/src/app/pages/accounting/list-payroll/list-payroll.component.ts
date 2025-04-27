import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  PaymentMethodType,
  PayrollDto,
  SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";

@Component({
  selector: "app-list-payroll",
  standalone: true,
  templateUrl: "./list-payroll.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    InputTextModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    PaymentStatusTagComponent,
  ],
})
export class ListPayrollComponent {
  public payrolls: PayrollDto[] = [];
  public isLoading = false;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {}

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getPayrolls({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.payrolls = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService
      .getPayrolls({
        orderBy: sortField,
        page: page,
        pageSize: rows,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.payrolls = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
      });
  }

  getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }

    return paymentMethodTypeOptions.find((x) => x.value === enumValue)?.label ?? "N/A";
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
