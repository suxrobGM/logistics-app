import {CommonModule} from "@angular/common";
import {Component, OnInit, computed, input, signal} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterModule} from "@angular/router";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ButtonModule} from "primeng/button";
import {CalendarModule} from "primeng/calendar";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {SelectModule} from "primeng/select";
import {ApiService} from "@/core/api";
import {
  CreatePayrollInvoiceCommand,
  EmployeeDto,
  InvoiceDto,
  PaymentStatus,
  SalaryType,
  UpdatePayrollInvoiceCommand,
  salaryTypeOptions,
} from "@/core/api/models";
import {PreviewPayrollInvoicesQuery} from "@/core/api/models/invoice/preview-payroll-invoices.model";
import {ToastService} from "@/core/services";
import {ValidationSummary} from "@/shared/components";
import {PredefinedDateRanges} from "@/shared/utils";
import {DateUtils} from "@/shared/utils";

@Component({
  selector: "app-payroll-invoice-edit",
  templateUrl: "./payroll-invoice-edit.component.html",
  imports: [
    CommonModule,
    CardModule,
    ValidationSummary,
    RouterModule,
    AutoCompleteModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    CalendarModule,
    ButtonModule,
    //AddressForm,
    SelectModule,
  ],
})
export class PayrollInvoiceEditComponent implements OnInit {
  //readonly paymentStatusOptions = paymentStatusOptions;
  //readonly paymentMethodOptions = paymentMethodTypeOptions;
  readonly todayDate = new Date();
  readonly form: FormGroup<PayrollForm>;
  readonly id = input<string>();
  readonly isLoading = signal(false);
  readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  readonly selectedEmployee = signal<EmployeeDto | null>(null);
  readonly previewPayrollInvoice = signal<InvoiceDto | null>(null);
  readonly title = computed(() => (this.id() ? "Edit payroll" : "Add a new payroll"));

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
    const lastWeek = [
      PredefinedDateRanges.getLastWeek().startDate,
      PredefinedDateRanges.getLastWeek().endDate,
    ];

    this.form = new FormGroup<PayrollForm>({
      employee: new FormControl(null, {validators: Validators.required}),
      dateRange: new FormControl(lastWeek, {validators: Validators.required, nonNullable: true}),
      //paymentStatus: new FormControl(null),
      //paymentMethod: new FormControl(null),
      //paymentBillingAddress: new FormControl(null),
    });

    // this.form.get("paymentStatus")?.valueChanges.subscribe((status) => {
    //   this.setConditionalValidators(status);
    // });
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

  searchEmployee(event: {query: string}): void {
    this.apiService.getEmployees({search: event.query}).subscribe((result) => {
      if (result.data) {
        this.suggestedEmployees.set(result.data);
      }
    });
  }

  handleAutoCompleteSelectEvent(event: AutoCompleteSelectEvent): void {
    this.fetchPreviewPayrollInvoice(event.value);
  }

  fetchPreviewPayrollInvoice(employeeId: string): void {
    if (!this.form.valid) {
      return;
    }

    const query: PreviewPayrollInvoicesQuery = {
      employeeId: employeeId,
      periodStart: this.form.value.dateRange![0],
      periodEnd: this.form.value.dateRange![1],
    };

    this.apiService.invoiceApi.previewPayrollInvoice(query).subscribe((result) => {
      this.previewPayrollInvoice.set(result.data!);
    });
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

  getSalaryTypeDesc(salaryType: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === salaryType)?.label ?? "";
  }

  private setConditionalValidators(paymentStatus: PaymentStatus | null): void {
    if (!paymentStatus) {
      return;
    }

    const paymentMethodControl = this.form.get("paymentMethod");
    const billingAddressControl = this.form.get("paymentBillingAddress");

    if (paymentStatus === PaymentStatus.Paid) {
      paymentMethodControl?.setValidators(Validators.required);
      billingAddressControl?.setValidators(Validators.required);
    } else {
      paymentMethodControl?.clearValidators();
      billingAddressControl?.clearValidators();
    }
  }

  private fetchPayroll(): void {
    const invoiceId = this.id();
    if (!invoiceId) {
      return;
    }

    this.isLoading.set(true);
    this.apiService.invoiceApi.getInvoice(invoiceId).subscribe(({data: invoice}) => {
      if (invoice) {
        this.form.patchValue({
          employee: invoice.employee,
          dateRange: [new Date(invoice.periodStart!), new Date(invoice.periodEnd!)],
        });

        this.previewPayrollInvoice.set(invoice);
        this.selectedEmployee.set(invoice.employee!);
      }

      this.isLoading.set(false);
    });
  }

  private addPayroll(): void {
    if (!this.form.valid) {
      return;
    }

    this.isLoading.set(true);
    const command: CreatePayrollInvoiceCommand = {
      employeeId: this.form.value.employee!.id,
      periodStart: this.form.value.dateRange![0],
      periodEnd: this.form.value.dateRange![1],
    };

    this.apiService.invoiceApi.createPayrollInvoice(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new payroll invoice entry has been added successfully");
        this.router.navigateByUrl("/invoices/payroll");
      }

      this.isLoading.set(false);
    });
  }

  private updatePayroll(): void {
    this.isLoading.set(true);

    const commad: UpdatePayrollInvoiceCommand = {
      id: this.id()!,
      employeeId: this.form.value.employee!.id,
      periodStart: this.form.value.dateRange![0],
      periodEnd: this.form.value.dateRange![1],
    };

    this.apiService.invoiceApi.updatePayrollInvoice(commad).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A payroll data has been updated successfully");
        this.router.navigateByUrl("/invoices/payroll");
      }

      this.isLoading.set(false);
    });
  }

  // private fetchPaymentMethods() {
  //   this.isLoading.set(true);

  //   this.apiService.paymentApi.getPaymentMethods().subscribe((result) => {
  //     if (result.success) {
  //       this.paymentMethods.set(result.data!);
  //       console.log("Payment methods fetched successfully:", result.data);
  //     }

  //     this.isLoading.set(false);
  //   });
  // }
}

interface PayrollForm {
  employee: FormControl<EmployeeDto | null>;
  dateRange: FormControl<Date[]>;
  //paymentStatus: FormControl<PaymentStatus | null>;
  //paymentMethod: FormControl<PaymentMethodType | null>;
  //paymentBillingAddress: FormControl<AddressDto | null>;
}
