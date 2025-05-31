import {CommonModule} from "@angular/common";
import {Component, OnInit, ViewEncapsulation} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {SelectModule} from "primeng/select";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {EmployeeDto, SalaryType, UpdateEmployeeCommand, salaryTypeOptions} from "@/core/api/models";
import {AuthService} from "@/core/auth";
import {UserRole} from "@/core/enums";
import {ToastService} from "@/core/services";
import {ValidationSummaryComponent} from "@/shared/components";
import {NumberUtils} from "@/shared/utils";
import {ChangeRoleDialogComponent} from "../components";

@Component({
  selector: "app-edit-employee",
  templateUrl: "./edit-employee.component.html",
  styleUrl: "./edit-employee.component.css",
  imports: [
    ToastModule,
    ConfirmDialogModule,
    ChangeRoleDialogComponent,
    CardModule,
    CommonModule,
    ProgressSpinnerModule,
    ButtonModule,
    RouterLink,
    ReactiveFormsModule,
    ValidationSummaryComponent,
    SelectModule,
  ],
  providers: [ConfirmationService],
})
export class EditEmployeeComponent implements OnInit {
  public id!: string;
  public isLoading = false;
  public showUpdateDialog = false;
  public canChangeRole = false;
  public employee?: EmployeeDto;
  public salaryTypes = salaryTypeOptions;
  public form: FormGroup<UpdateEmployeeForm>;

  constructor(
    private readonly authService: AuthService,
    private readonly apiService: ApiService,
    private readonly confirmationService: ConfirmationService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute
  ) {
    this.form = new FormGroup<UpdateEmployeeForm>({
      salary: new FormControl<number>(0, {
        validators: Validators.compose([Validators.required, Validators.min(0)]),
        nonNullable: true,
      }),
      salaryType: new FormControl<SalaryType>(SalaryType.None, {
        validators: Validators.required,
        nonNullable: true,
      }),
    });

    this.form.get("salaryType")?.valueChanges.subscribe((selectedSalaryType) => {
      const salaryControl = this.form.get("salary");

      if (!salaryControl) {
        return;
      }

      if (selectedSalaryType === SalaryType.ShareOfGross) {
        salaryControl.setValidators([Validators.required, Validators.min(0), Validators.max(100)]);
        salaryControl.setValue(0);
      } else if (selectedSalaryType === SalaryType.None) {
        salaryControl.setValue(0);
      } else {
        salaryControl.setValidators([Validators.required, Validators.min(0)]);
      }

      salaryControl.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
    });

    this.fetchEmployee();
  }

  updateEmployee() {
    if (!this.form.valid) {
      return;
    }

    const salaryType = this.form.value.salaryType;
    const salary = this.form.value.salary;

    const command: UpdateEmployeeCommand = {
      userId: this.id,
      salary: salaryType === SalaryType.ShareOfGross ? NumberUtils.toRatio(salary ?? 0) : salary,
      salaryType: salaryType,
    };

    this.isLoading = true;
    this.apiService.updateEmployee(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("The employee data has been successfully saved");
      }

      this.isLoading = false;
    });
  }

  confirmToDelete() {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this employee from the company?",
      // accept: () => this.deleteLoad()
    });
  }

  getEmployeeRoleNames(): string {
    const roleNames = this.employee?.roles?.map((i) => i.displayName).join(",");
    return roleNames ? roleNames : "";
  }

  openUpdateDialog() {
    this.showUpdateDialog = true;
  }

  isShareOfGrossSalary() {
    return this.form.value.salaryType === SalaryType.ShareOfGross;
  }

  isNoneSalary() {
    return this.form.value.salaryType === SalaryType.None;
  }

  private fetchEmployee() {
    this.isLoading = true;

    this.apiService.getEmployee(this.id).subscribe((result) => {
      if (result.success && result.data) {
        this.employee = result.data;
        const employeeRoles = this.employee.roles?.map((i) => i.name);
        const user = this.authService.getUserData();
        this.evaluateCanChangeRole(user?.roles, employeeRoles);

        const salaryType = result.data.salaryType;
        const salary = result.data.salary;

        this.form.patchValue({
          salary:
            salaryType === SalaryType.ShareOfGross ? NumberUtils.toPercent(salary ?? 0) : salary,
          salaryType: salaryType,
        });
      }

      this.isLoading = false;
    });
  }

  private evaluateCanChangeRole(userRoles?: string[], employeeRoles?: string[]) {
    if (!userRoles) {
      this.canChangeRole = false;
      return;
    }

    if (!employeeRoles || employeeRoles.length < 1) {
      this.canChangeRole = true;
      return;
    }

    const employeeRole = employeeRoles[0];

    if (userRoles.includes(UserRole.AppSuperAdmin) || userRoles.includes(UserRole.AppAdmin)) {
      this.canChangeRole = true;
    } else if (userRoles.includes(UserRole.Owner) && employeeRole !== UserRole.Owner) {
      this.canChangeRole = true;
    } else if (
      userRoles.includes(UserRole.Manager) &&
      employeeRole !== UserRole.Owner &&
      employeeRole !== UserRole.Manager
    ) {
      this.canChangeRole = true;
    }
  }
}

interface UpdateEmployeeForm {
  salary: FormControl<number>;
  salaryType: FormControl<SalaryType>;
}
