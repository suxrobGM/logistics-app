import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToastModule } from "primeng/toast";
import { Api, getEmployeeById, updateEmployee } from "@/core/api";
import {
  type EmployeeDto,
  type SalaryType,
  type UpdateEmployeeCommand,
  salaryTypeOptions,
} from "@/core/api/models";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { CurrencyInput, LabeledField, UnitInput, ValidationSummary } from "@/shared/components";
import { UserRole } from "@/shared/models";
import { NumberUtils } from "@/shared/utils";
import { ChangeRoleDialogComponent } from "../components";

@Component({
  selector: "app-employee-edit",
  templateUrl: "./employee-edit.html",
  imports: [
    ToastModule,
    ConfirmDialogModule,
    ChangeRoleDialogComponent,
    CardModule,
    ProgressSpinnerModule,
    ButtonModule,
    RouterLink,
    ReactiveFormsModule,
    ValidationSummary,
    LabeledField,
    SelectModule,
    InputGroupModule,
    InputGroupAddonModule,
    InputTextModule,
    UnitInput,
    CurrencyInput,
  ],
})
export class EmployeeEditComponent implements OnInit {
  protected readonly form: FormGroup<UpdateEmployeeForm>;
  protected readonly salaryTypes = salaryTypeOptions;

  private readonly authService = inject(AuthService);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly id = input<string>();
  protected readonly isLoading = signal(false);
  protected readonly showUpdateDialog = signal(false);
  protected readonly canChangeRole = signal(false);
  protected readonly employee = signal<EmployeeDto | null>(null);

  constructor() {
    this.form = new FormGroup<UpdateEmployeeForm>({
      salary: new FormControl<number>(0, {
        validators: Validators.compose([Validators.required, Validators.min(0)]),
        nonNullable: true,
      }),
      salaryType: new FormControl<SalaryType>("none", {
        validators: Validators.required,
        nonNullable: true,
      }),
    });

    this.form
      .get("salaryType")
      ?.valueChanges.pipe(takeUntilDestroyed())
      .subscribe((selectedSalaryType) => {
        const salaryControl = this.form.get("salary");

        if (!salaryControl) {
          return;
        }

        if (selectedSalaryType === "share_of_gross") {
          salaryControl.setValidators([Validators.required, Validators.min(0), Validators.max(100)]);
          salaryControl.setValue(0);
          salaryControl.enable();
        } else if (selectedSalaryType === "none") {
          salaryControl.setValue(0);
          salaryControl.disable();
        } else {
          salaryControl.setValidators([Validators.required, Validators.min(0)]);
          salaryControl.enable();
        }

        salaryControl.updateValueAndValidity();
      });
  }

  ngOnInit(): void {
    this.fetchEmployee();
  }

  async updateEmployee(): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    const salaryType = this.form.value.salaryType;
    const salary = this.form.value.salary;

    const command: UpdateEmployeeCommand = {
      userId: this.id()!,
      salary: salaryType === "share_of_gross" ? NumberUtils.toRatio(salary ?? 0) : salary,
      salaryType: salaryType,
    };

    this.isLoading.set(true);
    await this.api.invoke(updateEmployee, {
      userId: this.id()!,
      body: command,
    });
    this.toastService.showSuccess("The employee data has been successfully saved");

    this.isLoading.set(false);
  }

  confirmToDelete(): void {
    this.toastService.confirmDelete("employee", () => {
      // TODO: implement delete employee
    });
  }

  getEmployeeRoleNames(): string {
    const roleNames = this.employee()
      ?.roles?.map((i) => i.displayName)
      .join(",");
    return roleNames ? roleNames : "";
  }

  openUpdateDialog(): void {
    this.showUpdateDialog.set(true);
  }

  isShareOfGrossSalary(): boolean {
    return this.form.value.salaryType === "share_of_gross";
  }

  isNoneSalary() {
    return this.form.value.salaryType === "none";
  }

  private async fetchEmployee(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getEmployeeById, { userId: this.id()! });
    if (result) {
      this.employee.set(result);
      const employeeRoles = this.employee()
        ?.roles?.map((i) => i.name)
        .filter((n): n is string => !!n);
      const user = this.authService.getUserData();
      this.evaluateCanChangeRole(user?.roles, employeeRoles);

      const salaryType = result.salaryType;
      const salary = result.salary;

      this.form.patchValue({
        salary: salaryType === "share_of_gross" ? NumberUtils.toPercent(salary ?? 0) : salary,
        salaryType: salaryType,
      });
    }

    this.isLoading.set(false);
  }

  private evaluateCanChangeRole(userRoles?: string[], employeeRoles?: string[]) {
    if (!userRoles) {
      this.canChangeRole.set(false);
      return;
    }

    if (!employeeRoles || employeeRoles.length < 1) {
      this.canChangeRole.set(true);
      return;
    }

    const employeeRole = employeeRoles[0];

    if (userRoles.includes(UserRole.AppSuperAdmin) || userRoles.includes(UserRole.AppAdmin)) {
      this.canChangeRole.set(true);
    } else if (userRoles.includes(UserRole.Owner) && employeeRole !== UserRole.Owner) {
      this.canChangeRole.set(true);
    } else if (
      userRoles.includes(UserRole.Manager) &&
      employeeRole !== UserRole.Owner &&
      employeeRole !== UserRole.Manager
    ) {
      this.canChangeRole.set(true);
    }
  }
}

interface UpdateEmployeeForm {
  salary: FormControl<number>;
  salaryType: FormControl<SalaryType>;
}
