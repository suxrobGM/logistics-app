import { CommonModule } from "@angular/common";
import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  getEmployees,
  getInvoiceById,
  previewPayrollInvoice,
  updatePayrollInvoice,
  type EmployeeDto,
  type InvoiceDto,
  type InvoiceLineItemDto,
  type SalaryType,
  type UpdatePayrollInvoiceCommand,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Grid, Stack, Typography, UiAutocompleteField, UiButton } from "@logistics/shared/ui";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { PageHeader, UiFormField } from "@/shared/components";
import { DateUtils, PredefinedDateRanges } from "@/shared/utils";
import { PayrollLineItemsTable, PayrollPaySummary } from "../../components";

interface PayrollFormValue {
  employee: EmployeeDto | null;
  dateRange: Date[];
}

@Component({
  selector: "app-payroll-invoice-edit",
  templateUrl: "./payroll-invoice-edit.html",
  imports: [
    CardModule,
    CommonModule,
    CurrencyFormatPipe,
    DatePicker,
    DividerModule,
    FormField,
    FormRoot,
    FormsModule,
    Grid,
    PageHeader,
    PayrollLineItemsTable,
    PayrollPaySummary,
    ProgressSpinnerModule,
    RouterModule,
    Stack,
    Typography,
    UiAutocompleteField,
    UiButton,
    UiFormField,
  ],
})
export class PayrollInvoiceEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly todayDate = new Date();

  protected readonly invoiceId = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);
  protected readonly invoice = signal<InvoiceDto | null>(null);
  protected readonly lineItems = signal<InvoiceLineItemDto[]>([]);

  private readonly lastWeek = PredefinedDateRanges.getLastWeek();

  protected readonly model = signal<PayrollFormValue>({
    employee: null,
    dateRange: [this.lastWeek.startDate, this.lastWeek.endDate],
  });

  /**
   * `[formRoot]` runs `submission.action` on submit. It marks the whole tree touched first, skips
   * the action while invalid, and drives `form().submitting()` — so there is no `isSaving` signal
   * and no `if (!form.valid) return` guard. `isLoading` is kept because it tracks the initial fetch.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.employee, { message: "Employee is required." });
      required(p.dateRange, { message: "Pay period is required." });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();
          const command: UpdatePayrollInvoiceCommand = {
            id: this.invoiceId(),
            employeeId: value.employee!.id ?? undefined,
            periodStart: value.dateRange[0].toISOString(),
            periodEnd: value.dateRange[1].toISOString(),
          };

          try {
            await this.api.invoke(updatePayrollInvoice, {
              id: this.invoiceId(),
              body: command,
            });
          } catch {
            this.toastService.showError("Failed to update payroll");
            return undefined;
          }
          this.toastService.showSuccess("Payroll data has been updated successfully");
          this.router.navigateByUrl("/payroll/invoices");
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.fetchPayroll();
  }

  onDateRangeChange(dates: Date[]): void {
    this.model.update((v) => ({ ...v, dateRange: dates }));
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
      this.invoice.set(result as InvoiceDto);
    }
  }

  getSalaryTypeDesc(salaryType?: SalaryType): string {
    if (!salaryType) return "N/A";
    return salaryTypeOptions.find((option) => option.value === salaryType)?.label ?? "";
  }

  onLineItemsChanged(): void {
    this.fetchPayroll();
  }

  private async fetchPayroll(): Promise<void> {
    const invoiceId = this.invoiceId();
    if (!invoiceId) {
      return;
    }

    this.isLoading.set(true);
    try {
      const invoice = await this.api.invoke(getInvoiceById, { id: invoiceId });
      if (invoice) {
        this.model.update((v) => ({
          ...v,
          employee: invoice.employee ?? null,
          dateRange: [new Date(invoice.periodStart!), new Date(invoice.periodEnd!)],
        }));

        this.invoice.set(invoice);
        this.selectedEmployee.set(invoice.employee!);
        this.lineItems.set(invoice.lineItems ? [...invoice.lineItems] : []);
      }
    } finally {
      this.isLoading.set(false);
    }
  }
}
