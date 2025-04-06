import {CommonModule} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {ActivatedRoute, Router, RouterModule} from "@angular/router";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ButtonModule} from "primeng/button";
import {CalendarModule} from "primeng/calendar";
import {CardModule} from "primeng/card";
import {DropdownModule} from "primeng/dropdown";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {AddressFormComponent, ValidationSummaryComponent} from "@/components";
import {ApiService} from "@/core/api";
import {
  EnumType,
  PaymentMethodEnum,
  PaymentStatus,
  PaymentStatusEnum,
  SalaryType,
  SalaryTypeEnum,
} from "@/core/enums";
import {
  AddressDto,
  CreatePayrollCommand,
  EmployeeDto,
  PayrollDto,
  UpdatePayrollCommand,
} from "@/core/models";
import {ToastService} from "@/core/services";
import {PredefinedDateRanges} from "@/core/utils";
import {DateUtils} from "@/core/utils";

@Component({
  selector: "app-edit-payroll",
  standalone: true,
  templateUrl: "./edit-payroll.component.html",
  imports: [
    CommonModule,
    CardModule,
    ValidationSummaryComponent,
    RouterModule,
    DropdownModule,
    AutoCompleteModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    CalendarModule,
    ButtonModule,
    AddressFormComponent,
  ],
})
export class EditPayrollComponent implements OnInit {
  public salaryType = SalaryType;
  public paymentStatus = PaymentStatus;
  public paymentStatuses = PaymentStatusEnum.toArray();
  public paymentMethods = PaymentMethodEnum.toArray();
  public title = "Edit payroll";
  public id: string | null = null;
  public isLoading = false;
  public todayDate = new Date();
  public form: FormGroup<PayrollForm>;
  public suggestedEmployees: EmployeeDto[] = [];
  public selectedEmployee?: EmployeeDto;
  public computedPayroll?: PayrollDto;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    const lastWeek = [
      PredefinedDateRanges.getLastWeek().startDate,
      PredefinedDateRanges.getLastWeek().endDate,
    ];

    this.form = new FormGroup<PayrollForm>({
      employee: new FormControl(null, {validators: Validators.required}),
      dateRange: new FormControl(lastWeek, {validators: Validators.required, nonNullable: true}),
      paymentStatus: new FormControl(null),
      paymentMethod: new FormControl(null),
      paymentBillingAddress: new FormControl(null),
    });

    this.form.get("paymentStatus")?.valueChanges.subscribe((status) => {
      this.setConditionalValidators(status?.value as PaymentStatus);
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
    });

    if (this.isEditMode()) {
      this.title = "Edit payroll";
      this.fetchPayroll();
    } else {
      this.title = "Add a new payroll";
    }
  }

  tryCalculatePayroll() {
    if (!DateUtils.isValidRange(this.form.value.dateRange) || !this.selectedEmployee) {
      return;
    }

    this.calculatePayrollForEmployee(this.selectedEmployee);
  }

  searchEmployee(event: {query: string}) {
    this.apiService.getEmployees({search: event.query}).subscribe((result) => {
      if (result.data) {
        this.suggestedEmployees = result.data;
      }
    });
  }

  handleAutoCompleteSelectEvent(event: AutoCompleteSelectEvent) {
    this.calculatePayrollForEmployee(event.value);
  }

  calculatePayrollForEmployee(employee: EmployeeDto) {
    if (!this.form.valid) {
      return;
    }

    this.selectedEmployee = employee;
    const query: CreatePayrollCommand = {
      employeeId: employee.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    };

    this.apiService.calculateEmployeePayroll(query).subscribe((result) => {
      this.computedPayroll = result.data;
    });
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    if (this.id) {
      this.updatePayroll();
    } else {
      this.addPayroll();
    }
  }

  isEditMode(): boolean {
    return this.id != null && this.id !== "";
  }

  getSalaryTypeDesc(salaryType: SalaryType): string {
    return SalaryTypeEnum.getValue(salaryType).description;
  }

  private setConditionalValidators(paymentStatus: PaymentStatus | null) {
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

  private fetchPayroll() {
    if (!this.id) {
      return;
    }

    this.isLoading = true;
    this.apiService.getPayroll(this.id).subscribe(({data: payroll}) => {
      if (payroll) {
        this.form.patchValue({
          employee: payroll.employee,
          dateRange: [new Date(payroll.startDate), new Date(payroll.endDate)],
          paymentMethod: PaymentMethodEnum.getValue(payroll.payment.method!),
          paymentStatus: PaymentStatusEnum.getValue(payroll.payment.status),
          paymentBillingAddress: payroll.payment.billingAddress,
        });

        this.computedPayroll = payroll;
        this.selectedEmployee = payroll.employee;
      }

      this.isLoading = false;
    });
  }

  private addPayroll() {
    if (!this.form.valid) {
      return;
    }

    this.isLoading = true;
    const command: CreatePayrollCommand = {
      employeeId: this.form.value.employee!.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    };

    this.apiService.createPayroll(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new payroll entry has been added successfully");
        this.router.navigateByUrl("/accounting/payrolls");
      }

      this.isLoading = false;
    });
  }

  private updatePayroll() {
    this.isLoading = true;

    const commad: UpdatePayrollCommand = {
      id: this.id!,
      employeeId: this.form.value.employee!.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    };

    this.apiService.updatePayroll(commad).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A payroll data has been updated successfully");
        this.router.navigateByUrl("/accounting/payrolls");
      }

      this.isLoading = false;
    });
  }
}

interface PayrollForm {
  employee: FormControl<EmployeeDto | null>;
  dateRange: FormControl<Date[]>;
  paymentStatus: FormControl<EnumType | null>;
  paymentMethod: FormControl<EnumType | null>;
  paymentBillingAddress: FormControl<AddressDto | null>;
}
