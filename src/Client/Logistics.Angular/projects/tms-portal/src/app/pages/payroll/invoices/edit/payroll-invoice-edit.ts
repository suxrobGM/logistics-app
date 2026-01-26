import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  getEmployees,
  getInvoiceById,
  previewPayrollInvoice,
  updatePayrollInvoice,
} from "@logistics/shared/api";
import type {
  EmployeeDto,
  InvoiceDto,
  InvoiceLineItemDto,
  SalaryType,
  UpdatePayrollInvoiceCommand,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { LabeledField, PageHeader } from "@/shared/components";
import { DateUtils, PredefinedDateRanges } from "@/shared/utils";
import { PayrollLineItemsTable, PayrollPaySummary } from "../../components";

@Component({
  selector: "app-payroll-invoice-edit",
  templateUrl: "./payroll-invoice-edit.html",
  imports: [
    CommonModule,
    CardModule,
    LabeledField,
    RouterModule,
    AutoCompleteModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    ButtonModule,
    DatePicker,
    DividerModule,
    PageHeader,
    PayrollLineItemsTable,
    PayrollPaySummary,
  ],
})
export class PayrollInvoiceEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly todayDate = new Date();
  protected readonly form: FormGroup<PayrollForm>;

  protected readonly invoiceId = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);
  protected readonly invoice = signal<InvoiceDto | null>(null);
  protected readonly lineItems = signal<InvoiceLineItemDto[]>([]);

  constructor() {
    const lastWeek = [
      PredefinedDateRanges.getLastWeek().startDate,
      PredefinedDateRanges.getLastWeek().endDate,
    ];

    this.form = new FormGroup<PayrollForm>({
      employee: new FormControl(null, { validators: Validators.required }),
      dateRange: new FormControl(lastWeek, { validators: Validators.required, nonNullable: true }),
    });
  }

  ngOnInit(): void {
    this.fetchPayroll();
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
      this.invoice.set(result as InvoiceDto);
    }
  }

  submit(): void {
    if (!this.form.valid) {
      return;
    }

    this.updatePayroll();
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
        this.form.patchValue({
          employee: invoice.employee,
          dateRange: [new Date(invoice.periodStart!), new Date(invoice.periodEnd!)],
        });

        this.invoice.set(invoice);
        this.selectedEmployee.set(invoice.employee!);
        this.lineItems.set(invoice.lineItems ? [...invoice.lineItems] : []);
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  private async updatePayroll(): Promise<void> {
    this.isSaving.set(true);

    const command: UpdatePayrollInvoiceCommand = {
      id: this.invoiceId()!,
      employeeId: this.form.value.employee!.id ?? undefined,
      periodStart: this.form.value.dateRange![0].toISOString(),
      periodEnd: this.form.value.dateRange![1].toISOString(),
    };

    try {
      await this.api.invoke(updatePayrollInvoice, {
        id: this.invoiceId()!,
        body: command,
      });
      this.toastService.showSuccess("Payroll data has been updated successfully");
      this.router.navigateByUrl("/payroll/invoices");
    } catch {
      this.toastService.showError("Failed to update payroll");
    } finally {
      this.isSaving.set(false);
    }
  }
}

interface PayrollForm {
  employee: FormControl<EmployeeDto | null>;
  dateRange: FormControl<Date[]>;
}
