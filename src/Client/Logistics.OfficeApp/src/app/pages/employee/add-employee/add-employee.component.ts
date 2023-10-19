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
import {EnumValue, SalaryType, SalaryTypeEnum, convertEnumToArray} from '@core/enums';
import {CreateEmployee, Role, User} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {UserService} from '../services';


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
  ],
  providers: [
    UserService,
  ],
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: User[];
  public form: FormGroup<CreateEmployeeForm>;
  public roles: Role[];
  public salaryTypes: EnumValue[];
  public isLoading: boolean;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private userService: UserService)
  {
    this.suggestedUsers = [];
    this.roles = [];
    this.isLoading = false;
    this.salaryTypes = convertEnumToArray(SalaryTypeEnum);

    this.form = new FormGroup<CreateEmployeeForm>({
      user: new FormControl(null, {validators: Validators.required}),
      role: new FormControl(null, {validators: Validators.required}),
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
      role: {name: '', displayName: ' '},
    });
  }

  submit() {
    const user = this.form.value.user as User;

    if (!user) {
      this.toastService.showError('Select user');
      return;
    }

    const newEmployee: CreateEmployee = {
      userId: user.id,
      role: this.form.value.role?.name,
      salary: this.form.value.salary ?? 0,
      salaryType: this.form.value.salaryType ?? SalaryType.None,
    };

    this.isLoading = true;
    this.apiService.createEmployee(newEmployee).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('New employee has been added successfully');
        this.form.reset();
      }

      this.isLoading = false;
    });
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
  user: FormControl<User | null>;
  role: FormControl<Role | null>;
  salary: FormControl<number>;
  salaryType: FormControl<SalaryType>;
}
