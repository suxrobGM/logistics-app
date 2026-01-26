import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { UserRole } from "@logistics/shared";
import { Api, deleteEmployee, getEmployeeById, updateEmployee } from "@logistics/shared/api";
import {
  type EmployeeDto,
  type SalaryType,
  type UpdateEmployeeCommand,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToastModule } from "primeng/toast";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { CurrencyInput, LabeledField, UnitInput, ValidationSummary } from "@/shared/components";
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
  private readonly router = inject(Router);

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
          salaryControl.setValidators([
            Validators.required,
            Validators.min(0),
            Validators.max(100),
          ]);
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
    try {
      await this.api.invoke(updateEmployee, {
        userId: this.id()!,
        body: command,
      });
      this.toastService.showSuccess("The employee data has been successfully saved");
    } catch {
      this.toastService.showError("An error occurred while saving employee data");
    } finally {
      this.isLoading.set(false);
    }
  }

  confirmToDelete(): void {
    this.toastService.confirmDelete("employee", async () => {
      try {
        await this.api.invoke(deleteEmployee, { userId: this.id()! });
        this.toastService.showSuccess("Employee has been removed from the tenant");
        this.router.navigate(["/employees"]);
      } catch {
        this.toastService.showError("Failed to remove employee");
      }
    });
  }

  getEmployeeRoleName(): string {
    return this.employee()?.role?.displayName ?? "";
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
      const employeeRole = this.employee()?.role?.name;
      const user = this.authService.getUserData();
      this.evaluateCanChangeRole(user?.role, employeeRole);

      const salaryType = result.salaryType;
      const salary = result.salary;

      this.form.patchValue({
        salary: salaryType === "share_of_gross" ? NumberUtils.toPercent(salary ?? 0) : salary,
        salaryType: salaryType,
      });
    }

    this.isLoading.set(false);
  }

  private evaluateCanChangeRole(userRole?: string | null, employeeRole?: string | null) {
    if (!userRole) {
      this.canChangeRole.set(false);
      return;
    }

    if (!employeeRole) {
      this.canChangeRole.set(true);
      return;
    }

    if (userRole === UserRole.AppSuperAdmin || userRole === UserRole.AppAdmin) {
      this.canChangeRole.set(true);
    } else if (userRole === UserRole.Owner && employeeRole !== UserRole.Owner) {
      this.canChangeRole.set(true);
    } else if (
      userRole === UserRole.Manager &&
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
