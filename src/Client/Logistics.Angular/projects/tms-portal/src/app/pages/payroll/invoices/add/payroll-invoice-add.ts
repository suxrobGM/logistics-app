import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  batchCreatePayrollInvoices,
  createPayrollInvoice,
  getEmployees,
  previewPayrollInvoice,
  type BatchCreatePayrollInvoicesResult,
  type CreatePayrollInvoiceCommand,
  type EmployeeDto,
  type InvoiceDto,
  type SalaryType,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  DateRangePicker,
  Divider,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiAutocompleteField,
  UiButton,
  UiDataTable,
  UiMultiSelectField,
  UiToggleGroup,
  UiTooltip,
  type IconName,
  type UiToggleOption,
} from "@logistics/shared/ui";
import { PredefinedDateRanges } from "@logistics/shared/utils";
import { ToastService } from "@/core/services";
import { PageHeader, UiFormField, ValidatedForm } from "@/shared/components";
import { DateUtils } from "@/shared/utils";

type PayrollMode = "single" | "bulk";

interface BulkPreview {
  employee: EmployeeDto;
  preview: InvoiceDto | null;
  loading: boolean;
  error?: string;
}

interface PayrollFormValue {
  employee: EmployeeDto | null;
  dateRange: Date[];
}

@Component({
  selector: "app-payroll-invoice-add",
  templateUrl: "./payroll-invoice-add.html",
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DateRangePicker,
    Divider,
    FormField,
    FormRoot,
    Grid,
    Icon,
    PageHeader,
    RouterModule,
    Stack,
    Surface,
    Typography,
    UiAutocompleteField,
    UiButton,
    UiDataTable,
    UiFormField,
    UiMultiSelectField,
    UiToggleGroup,
    UiTooltip,
    ValidatedForm,
  ],
})
export class PayrollInvoiceAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly todayDate = new Date();

  private readonly lastWeek = PredefinedDateRanges.getLastWeek();

  protected readonly mode = signal<PayrollMode>("single");
  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);
  protected readonly previewPayrollInvoice = signal<InvoiceDto | null>(null);
  protected readonly allEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployees = signal<EmployeeDto[]>([]);
  protected readonly bulkPreviews = signal<BulkPreview[]>([]);

  protected readonly model = signal<PayrollFormValue>({
    employee: null,
    dateRange: [this.lastWeek.startDate, this.lastWeek.endDate],
  });

  /** Employee is only required in single mode; bulk mode validates its own selection. */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.employee, {
        when: () => this.mode() === "single",
        message: "Employee is required.",
      });
      required(p.dateRange, { message: "Pay period is required." });
    },
    {
      submission: {
        action: async () => {
          if (this.mode() === "single") {
            await this.addSinglePayroll();
          } else {
            await this.addBulkPayrolls();
          }
          return undefined;
        },
      },
    },
  );

  // Annotated, not inferred: without it `value` widens to `string`, ui-toggle-group's generic
  // resolves to `UiToggleGroup<string>`, and `(valueChange)` would hand `onModeChange` a plain
  // string instead of a PayrollMode.
  protected readonly modeOptions: UiToggleOption<PayrollMode>[] = [
    { label: "Single Employee", value: "single", icon: "user" as IconName },
    { label: "Multiple Employees", value: "bulk", icon: "users" as IconName },
  ];

  constructor() {
    this.loadAllEmployees();
  }

  onDateRangeChange(dates: Date[]): void {
    this.model.update((v) => ({ ...v, dateRange: dates }));
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

    if (!DateUtils.isValidRange(this.model().dateRange) || !employeeId) {
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

  handleAutoCompleteSelectEvent(employee: EmployeeDto | null): void {
    if (!employee) {
      return;
    }
    this.selectedEmployee.set(employee);
    this.fetchPreviewPayrollInvoice(employee.id!);
  }

  async fetchPreviewPayrollInvoice(employeeId: string): Promise<void> {
    if (!this.form().valid()) {
      return;
    }

    const dateRange = this.model().dateRange;
    const result = await this.api.invoke(previewPayrollInvoice, {
      EmployeeId: employeeId,
      PeriodStart: dateRange[0].toISOString(),
      PeriodEnd: dateRange[1].toISOString(),
    });
    if (result) {
      this.previewPayrollInvoice.set(result as InvoiceDto);
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
    const dateRange = this.model().dateRange;
    if (employees.length === 0 || !DateUtils.isValidRange(dateRange)) {
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
            PeriodStart: dateRange[0].toISOString(),
            PeriodEnd: dateRange[1].toISOString(),
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
    const value = this.model();
    const command: CreatePayrollInvoiceCommand = {
      employeeId: value.employee!.id ?? undefined,
      periodStart: value.dateRange[0].toISOString(),
      periodEnd: value.dateRange[1].toISOString(),
    };

    try {
      await this.api.invoke(createPayrollInvoice, { body: command });
      this.toastService.showSuccess("Payroll invoice created successfully");
      this.router.navigateByUrl("/payroll/invoices");
    } catch {
      this.toastService.showError("Failed to create payroll invoice");
    }
  }

  private async addBulkPayrolls(): Promise<void> {
    const employees = this.selectedEmployees();
    const dateRange = this.model().dateRange;
    if (employees.length === 0 || !DateUtils.isValidRange(dateRange)) {
      this.toastService.showError("Please select at least one employee and a valid date range");
      return;
    }

    try {
      const result = await this.api.invoke(batchCreatePayrollInvoices, {
        body: {
          employeeIds: employees.map((e) => e.id!),
          periodStart: dateRange[0].toISOString(),
          periodEnd: dateRange[1].toISOString(),
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
    }
  }
}
