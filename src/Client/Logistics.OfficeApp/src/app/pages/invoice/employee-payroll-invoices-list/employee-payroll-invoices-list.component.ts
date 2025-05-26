import {CommonModule} from "@angular/common";
import {Component, OnInit, input, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {InvoiceStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  EmployeeDto,
  InvoiceDto,
  PaymentMethodType,
  SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";

@Component({
  selector: "app-employee-payroll-invoices-list",
  templateUrl: "./employee-payroll-invoices-list.component.html",
  imports: [
    CommonModule,
    CardModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    RouterModule,
    InvoiceStatusTagComponent,
    ProgressSpinnerModule,
  ],
})
export class EmployeePayrollInvoicesListComponent implements OnInit {
  readonly employeeId = input.required<string>();
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly employee = signal<EmployeeDto | null>(null);
  readonly isLoadingEmployee = signal(false);
  readonly isLoadingPayrolls = signal(false);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

  ngOnInit(): void {
    this.fetchEmployee();
  }

  load(event: TableLazyLoadEvent): void {
    this.isLoadingPayrolls.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.invoiceApi
      .getInvoices({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        employeeId: this.employeeId(),
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.invoices.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoadingPayrolls.set(false);
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

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown";
  }

  private fetchEmployee(): void {
    this.isLoadingEmployee.set(true);

    this.apiService.getEmployee(this.employeeId()).subscribe((result) => {
      if (result.data) {
        this.employee.set(result.data);
      }

      this.isLoadingEmployee.set(false);
    });
  }
}
