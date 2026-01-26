import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  batchCreatePayrollInvoices,
  createPayrollInvoice,
  getEmployees,
  previewPayrollInvoice,
} from "@logistics/shared/api";
import {
  type BatchCreatePayrollInvoicesResult,
  type CreatePayrollInvoiceCommand,
  type EmployeeDto,
  type InvoiceDto,
  type SalaryType,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { DateRangePicker } from "@logistics/shared/components";
import { PredefinedDateRanges } from "@logistics/shared/utils";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { MultiSelectModule } from "primeng/multiselect";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { SelectButtonModule } from "primeng/selectbutton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { LabeledField, PageHeader, ValidationSummary } from "@/shared/components";
import { DateUtils } from "@/shared/utils";

type PayrollMode = "single" | "bulk";

interface BulkPreview {
  employee: EmployeeDto;
  preview: InvoiceDto | null;
  loading: boolean;
  error?: string;
}

@Component({
  selector: "app-payroll-invoice-add",
  templateUrl: "./payroll-invoice-add.html",
  imports: [
    CommonModule,
    CardModule,
    ValidationSummary,
    LabeledField,
    RouterModule,
    AutoCompleteModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    DateRangePicker,
    DividerModule,
    SelectButtonModule,
    MultiSelectModule,
    TableModule,
    TooltipModule,
    PageHeader,
  ],
})
export class PayrollInvoiceAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly todayDate = new Date();
  protected readonly form: FormGroup<PayrollForm>;

  protected readonly isLoading = signal(false);
  protected readonly isBulkCreating = signal(false);
  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);
  protected readonly previewPayrollInvoice = signal<InvoiceDto | null>(null);
  protected readonly mode = signal<PayrollMode>("single");
  protected readonly allEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployees = signal<EmployeeDto[]>([]);
  protected readonly bulkPreviews = signal<BulkPreview[]>([]);

  protected readonly modeOptions = [
    { label: "Single Employee", value: "single", icon: "pi pi-user" },
    { label: "Multiple Employees", value: "bulk", icon: "pi pi-users" },
  ];

  constructor() {
    const lastWeek = PredefinedDateRanges.getLastWeek();

    this.form = new FormGroup<PayrollForm>({
      employee: new FormControl(null, { validators: Validators.required }),
      dateRange: new FormControl([lastWeek.startDate, lastWeek.endDate], {
        validators: Validators.required,
        nonNullable: true,
      }),
    });

    this.loadAllEmployees();
  }

  onDateRangeChange(dates: Date[]): void {
    this.form.patchValue({ dateRange: dates });
    this.tryCalculatePayroll();
    this.refreshBulkPreviews();
  }

  onModeChange(newMode: PayrollMode): void {
    this.mode.set(newMode);
    if (newMode === "bulk") {
      this.refreshBulkPreviews();
    }
  }

  onEmployeesSelected(): void {
    this.refreshBulkPreviews();
  }

  tryCalculatePayroll(): void {
    const employeeId = this.selectedEmployee()?.id;

    if (!DateUtils.isValidRange(this.form.value.dateRange) || !employeeId) {
      return;
    }

    this.fetchPreviewPayrollInvoice(employeeId);
  }

  async searchEmployee(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getEmployees, { Search: event.query });
    if (result.items) {
      this.suggestedEmployees.set(result.items);
    }
  }

  handleAutoCompleteSelectEvent(event: AutoCompleteSelectEvent): void {
    const employee = event.value as EmployeeDto;
    this.selectedEmployee.set(employee);
    this.fetchPreviewPayrollInvoice(employee.id!);
  }

  async fetchPreviewPayrollInvoice(employeeId: string): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    const result = await this.api.invoke(previewPayrollInvoice, {
      EmployeeId: employeeId,
      PeriodStart: this.form.value.dateRange![0].toISOString(),
      PeriodEnd: this.form.value.dateRange![1].toISOString(),
    });
    if (result) {
      this.previewPayrollInvoice.set(result as InvoiceDto);
    }
  }

  submit(): void {
    if (this.mode() === "single") {
      if (!this.form.valid) {
        return;
      }
      this.addSinglePayroll();
    } else {
      this.addBulkPayrolls();
    }
  }

  getSalaryTypeDesc(salaryType?: SalaryType): string {
    if (!salaryType) return "N/A";
    return salaryTypeOptions.find((option) => option.value === salaryType)?.label ?? "";
  }

  getBulkTotalAmount(): number {
    return this.bulkPreviews()
      .filter((p) => p.preview)
      .reduce((sum, p) => sum + (p.preview?.total?.amount ?? 0), 0);
  }

  isBulkCreateDisabled(): boolean {
    return this.selectedEmployees().length === 0 || this.bulkPreviews().some((p) => p.loading);
  }

  onSelectedEmployeesChange(employees: EmployeeDto[]): void {
    this.selectedEmployees.set(employees);
    this.onEmployeesSelected();
  }

  private async loadAllEmployees(): Promise<void> {
    const result = await this.api.invoke(getEmployees, { PageSize: 500 });
    if (result.items) {
      this.allEmployees.set(result.items);
    }
  }

  private async refreshBulkPreviews(): Promise<void> {
    const employees = this.selectedEmployees();
    if (employees.length === 0 || !DateUtils.isValidRange(this.form.value.dateRange)) {
      this.bulkPreviews.set([]);
      return;
    }

    // Initialize previews with loading state
    const previews: BulkPreview[] = employees.map((emp) => ({
      employee: emp,
      preview: null,
      loading: true,
    }));
    this.bulkPreviews.set(previews);

    // Fetch previews for all selected employees
    const updatedPreviews = await Promise.all(
      employees.map(async (employee) => {
        try {
          const result = await this.api.invoke(previewPayrollInvoice, {
            EmployeeId: employee.id!,
            PeriodStart: this.form.value.dateRange![0].toISOString(),
            PeriodEnd: this.form.value.dateRange![1].toISOString(),
          });
          return {
            employee,
            preview: result as InvoiceDto | null,
            loading: false,
          };
        } catch {
          return {
            employee,
            preview: null,
            loading: false,
            error: "Failed to preview",
          };
        }
      }),
    );

    this.bulkPreviews.set(updatedPreviews);
  }

  private async addSinglePayroll(): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    this.isLoading.set(true);
    const command: CreatePayrollInvoiceCommand = {
      employeeId: this.form.value.employee!.id ?? undefined,
      periodStart: this.form.value.dateRange![0].toISOString(),
      periodEnd: this.form.value.dateRange![1].toISOString(),
    };

    try {
      await this.api.invoke(createPayrollInvoice, { body: command });
      this.toastService.showSuccess("Payroll invoice created successfully");
      this.router.navigateByUrl("/payroll/invoices");
    } catch {
      this.toastService.showError("Failed to create payroll invoice");
    } finally {
      this.isLoading.set(false);
    }
  }

  private async addBulkPayrolls(): Promise<void> {
    const employees = this.selectedEmployees();
    if (employees.length === 0 || !DateUtils.isValidRange(this.form.value.dateRange)) {
      this.toastService.showError("Please select at least one employee and a valid date range");
      return;
    }

    this.isBulkCreating.set(true);
    try {
      const result = await this.api.invoke(batchCreatePayrollInvoices, {
        body: {
          employeeIds: employees.map((e) => e.id!),
          periodStart: this.form.value.dateRange![0].toISOString(),
          periodEnd: this.form.value.dateRange![1].toISOString(),
        },
      });

      const typedResult = result as BatchCreatePayrollInvoicesResult;
      const createdCount = typedResult.createdInvoiceIds?.length ?? 0;
      const errorCount = typedResult.errors?.length ?? 0;

      if (createdCount > 0 && errorCount === 0) {
        this.toastService.showSuccess(`${createdCount} payroll invoice(s) created successfully`);
        this.router.navigateByUrl("/payroll/invoices");
      } else if (createdCount > 0 && errorCount > 0) {
        this.toastService.showInfo(
          `${createdCount} payroll(s) created, ${errorCount} failed. Check the list for details.`,
        );
        this.router.navigateByUrl("/payroll/invoices");
      } else {
        const errorMessages = typedResult.errors?.map((e) => e.message).join("; ");
        this.toastService.showError(`Failed to create payrolls: ${errorMessages}`);
      }
    } catch {
      this.toastService.showError("Failed to create payroll invoices");
    } finally {
      this.isBulkCreating.set(false);
    }
  }
}

interface PayrollForm {
  employee: FormControl<EmployeeDto | null>;
  dateRange: FormControl<Date[]>;
}
