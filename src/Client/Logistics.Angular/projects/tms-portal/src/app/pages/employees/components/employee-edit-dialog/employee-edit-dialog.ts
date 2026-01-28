import { Component, effect, inject, input, model, output, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { UserRole } from "@logistics/shared";
import { Api, updateEmployee } from "@logistics/shared/api";
import type { EmployeeDto, EmployeeStatus, SalaryType, UpdateEmployeeCommand } from "@logistics/shared/api";
import { employeeStatusOptions, salaryTypeOptions } from "@logistics/shared/api/enums";
import { AccordionModule } from "primeng/accordion";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputGroupModule } from "primeng/inputgroup";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { AuthService } from "@/core/auth";
import { CurrencyInput, LabeledField, UnitInput, ValidationSummary } from "@/shared/components";
import { NumberUtils } from "@/shared/utils";
import { ChangeRoleDialog } from "../change-role-dialog/change-role-dialog";

@Component({
  selector: "app-employee-edit-dialog",
  templateUrl: "./employee-edit-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    ReactiveFormsModule,
    SelectModule,
    InputGroupModule,
    InputTextModule,
    AccordionModule,
    LabeledField,
    UnitInput,
    CurrencyInput,
    ValidationSummary,
    ChangeRoleDialog,
  ],
})
export class EmployeeEditDialog {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);

  readonly visible = model<boolean>(false);
  readonly employee = input<EmployeeDto | null>(null);
  readonly saved = output<void>();
  readonly deleted = output<void>();

  protected readonly form: FormGroup<UpdateEmployeeForm>;
  protected readonly salaryTypes = salaryTypeOptions;
  protected readonly statusOptions = employeeStatusOptions;
  protected readonly isLoading = signal(false);
  protected readonly canChangeRole = signal(false);
  protected readonly changeRoleDialogVisible = signal(false);

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
      status: new FormControl<EmployeeStatus>("active", {
        validators: Validators.required,
        nonNullable: true,
      }),
    });

    this.form
      .get("salaryType")
      ?.valueChanges.pipe(takeUntilDestroyed())
      .subscribe((selectedSalaryType) => {
        const salaryControl = this.form.get("salary");
        if (!salaryControl) return;

        if (selectedSalaryType === "share_of_gross") {
          salaryControl.setValidators([
            Validators.required,
            Validators.min(0),
            Validators.max(100),
          ]);
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

    effect(() => {
      const emp = this.employee();
      if (emp && this.visible()) {
        this.populateForm(emp);
        this.evaluateCanChangeRole(emp);
      }
    });
  }

  async save(): Promise<void> {
    if (!this.form.valid) return;

    const emp = this.employee();
    if (!emp?.id) return;

    const salaryType = this.form.value.salaryType!;
    const salary = this.form.value.salary!;
    const status = this.form.value.status!;

    const command: UpdateEmployeeCommand = {
      userId: emp.id,
      salary: salaryType === "share_of_gross" ? NumberUtils.toRatio(salary) : salary,
      salaryType: salaryType,
      status: status,
    };

    this.isLoading.set(true);
    try {
      await this.api.invoke(updateEmployee, {
        userId: emp.id,
        body: command,
      });
      this.saved.emit();
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
  }

  openChangeRoleDialog(): void {
    this.changeRoleDialogVisible.set(true);
  }

  onRoleChanged(): void {
    this.changeRoleDialogVisible.set(false);
    this.saved.emit();
  }

  isShareOfGrossSalary(): boolean {
    return this.form.value.salaryType === "share_of_gross";
  }

  isNoneSalary(): boolean {
    return this.form.value.salaryType === "none";
  }

  private populateForm(emp: EmployeeDto): void {
    const salaryType = emp.salaryType ?? "none";
    const salary = emp.salary ?? 0;

    this.form.patchValue({
      salary: salaryType === "share_of_gross" ? NumberUtils.toPercent(salary) : salary,
      salaryType: salaryType,
      status: emp.status ?? "active",
    });
  }

  private evaluateCanChangeRole(emp: EmployeeDto): void {
    const user = this.authService.getUserData();
    const userRole = user?.role;
    const employeeRole = emp.role?.name;

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
    } else {
      this.canChangeRole.set(false);
    }
  }
}

interface UpdateEmployeeForm {
  salary: FormControl<number>;
  salaryType: FormControl<SalaryType>;
  status: FormControl<EmployeeStatus>;
}
