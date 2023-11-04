/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormGroup, FormControl, Validators, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
import {CardModule} from 'primeng/card';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {CalendarModule} from 'primeng/calendar';
import {CreatePayroll, Employee, Payroll, UpdatePayroll} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {PredefinedDateRanges} from '@core/helpers';
import {
  convertEnumToArray,
  PaymentStatusEnum,
  PaymentMethodEnum,
  SalaryType,
  SalaryTypeEnum,
} from '@core/enums';
import {ValidationSummaryComponent} from '@shared/components';


@Component({
  selector: 'app-edit-payroll',
  standalone: true,
  templateUrl: './edit-payroll.component.html',
  styleUrls: ['./edit-payroll.component.scss'],
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
  ],
})
export class EditPayrollComponent implements OnInit {
  public readonly paymentStatuses = convertEnumToArray(PaymentStatusEnum);
  public readonly paymentMethods = convertEnumToArray(PaymentMethodEnum);
  public title = 'Edit payroll';
  public id: string | null = null;
  public isLoading = false;
  public todayDate = new Date();
  public form: FormGroup<PayrollForm>;
  public suggestedEmployees: Employee[] = [];
  public selectedEmployee?: Employee;
  public computedPayroll?: Payroll;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute,
    private readonly router: Router)
  {
    const lastWeek = [PredefinedDateRanges.getLastWeek().startDate, PredefinedDateRanges.getLastWeek().endDate]
  
    this.form = new FormGroup<PayrollForm>({
      employee: new FormControl<Employee | null>(null, {validators: Validators.required}),
      dateRange: new FormControl<Date[]>(lastWeek, {validators: Validators.required, nonNullable: true}),
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    if (this.isEditMode()) {
      this.title = 'Edit payroll';
      this.fetchPayment();
    }
    else {
      this.title = 'Add a new payroll';
    }
  }
  
  selectDate(event: any) {
    console.log(event);
    
  }

  searchEmployee(event: {query: string}) {
    this.apiService.getEmployees({search: event.query}).subscribe((result) => {
      if (result.data) {
        this.suggestedEmployees = result.data;
      }
    });
  }

  calculatePayrollForEmployee(employee: Employee) {
    if (!this.form.valid) {
      return;
    }

    this.selectedEmployee = employee;
    const query: CreatePayroll = {
      employeeId: employee.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    }

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
    }
    else {
      this.addPayroll();
    }
  }

  isEditMode(): boolean {
    return this.id != null && this.id !== '';
  }

  isShareOfGrossSalary() {
    return this.selectedEmployee?.salaryType === SalaryType.ShareOfGross;
  }

  getSalaryTypeDesc(salaryType: SalaryType): string {
    return SalaryTypeEnum.getDescription(salaryType);
  }

  private fetchPayment() {
    if (!this.id) {
      return;
    }

    this.isLoading = true;
    this.apiService.getPayroll(this.id).subscribe(({data: payroll}) => {
      if (payroll) {
        this.form.patchValue({
          employee: payroll.employee,
          dateRange: [new Date(payroll.startDate), new Date(payroll.endDate)],
        });
      }

      this.isLoading = false;
    })
  }

  private addPayroll() {
    if (!this.form.valid) {
      return;
    }

    this.isLoading = true;
    const command: CreatePayroll = {
      employeeId: this.form.value.employee!.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    }

    this.apiService.createPayroll(command).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A new payroll entry has been added successfully');
        this.router.navigateByUrl('/accounting/payrolls');
      }

      this.isLoading = false;
    });
  }

  private updatePayroll() {
    this.isLoading = true;

    const commad: UpdatePayroll = {
      id: this.id!,
      employeeId: this.form.value.employee!.id,
      startDate: this.form.value.dateRange![0],
      endDate: this.form.value.dateRange![1],
    }

    this.apiService.updatePayroll(commad).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A payroll data has been updated successfully');
        this.router.navigateByUrl('/accounting/payrolls');
      }

      this.isLoading = false;
    });
  }
}

interface PayrollForm {
  employee: FormControl<Employee | null>;
  dateRange: FormControl<Date[]>;
}
