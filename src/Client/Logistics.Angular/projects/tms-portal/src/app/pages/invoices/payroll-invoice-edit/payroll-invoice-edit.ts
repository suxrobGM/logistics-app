import { CommonModule } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import {
  Api,
  createPayrollInvoice,
  getEmployees,
  getInvoiceById,
  previewPayrollInvoice,
  updatePayrollInvoice,
} from "@/core/api";
import {
  type CreatePayrollInvoiceCommand,
  type EmployeeDto,
  type InvoiceDto,
  type PaymentStatus,
  type SalaryType,
  type UpdatePayrollInvoiceCommand,
  salaryTypeOptions,
} from "@/core/api/models";
import { ToastService } from "@/core/services";
import { ValidationSummary } from "@/shared/components";
import { PredefinedDateRanges } from "@/shared/utils";
import { DateUtils } from "@/shared/utils";

@Component({
  selector: "app-payroll-invoice-edit",
  templateUrl: "./payroll-invoice-edit.html",
  imports: [
    CommonModule,
    CardModule,
    ValidationSummary,
    RouterModule,
    AutoCompleteModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    DatePicker,
    DividerModule,
  ],
})
export class PayrollInvoiceEditComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly todayDate = new Date();
  protected readonly form: FormGroup<PayrollForm>;
  protected readonly id = input<string>();
  protected readonly isLoading = signal(false);
  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);
  protected readonly previewPayrollInvoice = signal<InvoiceDto | null>(null);
  protected readonly title = computed(() => (this.id() ? "Edit payroll" : "Add a new payroll"));

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
    if (this.id()) {
      this.fetchPayroll();
    }
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
    this.fetchPreviewPayrollInvoice(event.value);
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
    if (!this.form.valid) {
      return;
    }

    if (this.id()) {
      this.updatePayroll();
    } else {
      this.addPayroll();
    }
  }

  getSalaryTypeDesc(salaryType?: SalaryType): string {
    if (!salaryType) return "N/A";
    return salaryTypeOptions.find((option) => option.value === salaryType)?.label ?? "";
  }

  private setConditionalValidators(paymentStatus: PaymentStatus | null): void {
    if (!paymentStatus) {
      return;
    }

    const paymentMethodControl = this.form.get("paymentMethod");
    const billingAddressControl = this.form.get("paymentBillingAddress");

    if (paymentStatus === "paid") {
      paymentMethodControl?.setValidators(Validators.required);
      billingAddressControl?.setValidators(Validators.required);
    } else {
      paymentMethodControl?.clearValidators();
      billingAddressControl?.clearValidators();
    }
  }

  private async fetchPayroll(): Promise<void> {
    const invoiceId = this.id();
    if (!invoiceId) {
      return;
    }

    this.isLoading.set(true);
    const invoice = await this.api.invoke(getInvoiceById, { id: invoiceId });
    if (invoice) {
      this.form.patchValue({
        employee: invoice.employee,
        dateRange: [new Date(invoice.periodStart!), new Date(invoice.periodEnd!)],
      });

      this.previewPayrollInvoice.set(invoice);
      this.selectedEmployee.set(invoice.employee!);
    }

    this.isLoading.set(false);
  }

  private async addPayroll(): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    this.isLoading.set(true);
    const command: CreatePayrollInvoiceCommand = {
      employeeId: this.form.value.employee!.id ?? undefined,
      periodStart: this.form.value.dateRange![0].toISOString(),
      periodEnd: this.form.value.dateRange![1].toISOString(),
    };

    await this.api.invoke(createPayrollInvoice, { body: command });
    this.toastService.showSuccess("A new payroll invoice entry has been added successfully");
    this.router.navigateByUrl("/invoices/payroll");

    this.isLoading.set(false);
  }

  private async updatePayroll(): Promise<void> {
    this.isLoading.set(true);

    const command: UpdatePayrollInvoiceCommand = {
      id: this.id()!,
      employeeId: this.form.value.employee!.id ?? undefined,
      periodStart: this.form.value.dateRange![0].toISOString(),
      periodEnd: this.form.value.dateRange![1].toISOString(),
    };

    await this.api.invoke(updatePayrollInvoice, {
      id: this.id()!,
      body: command,
    });
    this.toastService.showSuccess("A payroll data has been updated successfully");
    this.router.navigateByUrl("/invoices/payroll");

    this.isLoading.set(false);
  }
}

interface PayrollForm {
  employee: FormControl<EmployeeDto | null>;
  dateRange: FormControl<Date[]>;
}
