import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {CardModule} from 'primeng/card';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';
import {ConfirmationService} from 'primeng/api';
import {SalaryType, SalaryTypeEnum} from '@/core/enums';
import {CreateEmployeeCommand, RoleDto, UserDto} from '@/core/models';
import {ApiService, ToastService} from '@/core/services';
import {UserService} from '../services';
import {ValidationSummaryComponent} from '@/components';


@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    NgIf,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    DropdownModule,
    ButtonModule,
    RouterLink,
    ValidationSummaryComponent,
  ],
  providers: [
    UserService,
    ConfirmationService
  ],
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: UserDto[] = [];
  public form: FormGroup<CreateEmployeeForm>;
  public roles: RoleDto[] = [];
  public salaryTypes = SalaryTypeEnum.toArray();
  public isLoading = false;

  constructor(
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly userService: UserService)
  {
    this.form = new FormGroup<CreateEmployeeForm>({
      user: new FormControl(null, {validators: Validators.required}),
      role: new FormControl(null),
      salary: new FormControl<number>(0, {validators: Validators.required, nonNullable: true}),
      salaryType: new FormControl<SalaryType>(SalaryType.None, {validators: Validators.required, nonNullable: true})
    });
  }

  ngOnInit(): void {
    this.fetchRoles();
  }

  searchUser(event: {query: string}) {
    this.userService.searchUser(event.query).subscribe((users) => {
      if (users) {
        this.suggestedUsers = users;
      }
    });
  }

  clearSelctedRole() {
    this.form.patchValue({
      role: null,
    });
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    const user = this.form.value.user as UserDto;

    if (!user) {
      this.toastService.showError('Select user');
      return;
    }

    const newEmployee: CreateEmployeeCommand = {
      userId: user.id,
      role: this.form.value.role?.name,
      salary: this.form.value.salary ?? 0,
      salaryType: this.form.value.salaryType ?? SalaryType.None,
    };

    this.isLoading = true;
    this.apiService.createEmployee(newEmployee).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess('New employee has been added successfully');
        this.form.reset();
      }

      this.isLoading = false;
    });
  }

  isShareOfGrossSalary() {
    return this.form.value.salaryType === SalaryType.ShareOfGross;
  }

  isNoneSalary() {
    return this.form.value.salaryType === SalaryType.None;
  }

  private fetchRoles() {
    this.isLoading = true;

    this.userService.fetchRoles().subscribe((roles) => {
      if (roles) {
        this.roles = roles;
      }

      this.isLoading = false;
    });
  }
}

interface CreateEmployeeForm {
  user: FormControl<UserDto | null>;
  role: FormControl<RoleDto | null>;
  salary: FormControl<number>;
  salaryType: FormControl<SalaryType>;
}
