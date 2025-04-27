import {CommonModule} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {ActivatedRoute, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  EmployeeDto,
  PaymentMethodType,
  PayrollDto,
  SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";

@Component({
  selector: "app-view-employee-payrolls",
  standalone: true,
  templateUrl: "./view-employee-payrolls.component.html",
  imports: [
    CommonModule,
    CardModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    RouterModule,
    PaymentStatusTagComponent,
    ProgressSpinnerModule,
  ],
})
export class ViewEmployeePayrollsComponent implements OnInit {
  private employeeId!: string;
  public payrolls: PayrollDto[] = [];
  public employee?: EmployeeDto;
  public isLoadingEmployee = false;
  public isLoadingPayrolls = false;
  public totalRecords = 0;
  public first = 0;

  constructor(
    private readonly apiService: ApiService,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.employeeId = params["employeeId"];
    });

    this.fetchEmployee();
  }

  load(event: TableLazyLoadEvent) {
    this.isLoadingPayrolls = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService
      .getPayrolls({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        employeeId: this.employeeId,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.payrolls = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoadingPayrolls = false;
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

  private fetchEmployee() {
    this.isLoadingEmployee = true;

    this.apiService.getEmployee(this.employeeId).subscribe((result) => {
      if (result.data) {
        this.employee = result.data;
      }

      this.isLoadingEmployee = false;
    });
  }
}
