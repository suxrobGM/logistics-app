import { Component, computed, effect, inject, input, model, output, signal } from "@angular/core";
import { disabled, form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import { regionAllowedCountries, UserRole } from "@logistics/shared";
import {
  Api,
  updateEmployee,
  type Address,
  type EmployeeDto,
  type EmployeeStatus,
  type SalaryType,
  type UpdateEmployeeCommand,
} from "@logistics/shared/api";
import { employeeStatusOptions, salaryTypeOptions } from "@logistics/shared/api/enums";
import {
  AddressForm,
  Stack,
  UiAccordionImports,
  UiButton,
  UiDialog,
  UiTextField,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { TenantService } from "@/core/services/tenant.service";
import {
  CurrencyField,
  UiFormField,
  UiSelectField,
  UnitField,
  ValidatedForm,
} from "@/shared/components";
import { NumberUtils } from "@/shared/utils";
import { ChangeRoleDialog } from "../change-role-dialog/change-role-dialog";

interface EmployeeEditModel {
  salary: number;
  salaryType: SalaryType;
  status: EmployeeStatus;
  address: Address | null;
}

const EMPTY: EmployeeEditModel = {
  salary: 0,
  salaryType: "none",
  status: "active",
  address: null,
};

@Component({
  selector: "app-employee-edit-dialog",
  templateUrl: "./employee-edit-dialog.html",
  imports: [
    AddressForm,
    ChangeRoleDialog,
    CurrencyField,
    FormField,
    FormRoot,
    Stack,
    UiAccordionImports,
    UiButton,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiTextField,
    UnitField,
    ValidatedForm,
  ],
})
export class EmployeeEditDialog {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);

  protected readonly allowedCountries = computed(() =>
    regionAllowedCountries(this.tenantService.tenantData()?.settings?.region),
  );

  readonly visible = model<boolean>(false);
  readonly employee = input<EmployeeDto | null>(null);
  readonly saved = output<void>();
  readonly deleted = output<void>();

  protected readonly salaryTypes = salaryTypeOptions;
  protected readonly statusOptions = employeeStatusOptions;
  protected readonly canChangeRole = signal(false);
  protected readonly changeRoleDialogVisible = signal(false);

  protected readonly model = signal<EmployeeEditModel>({ ...EMPTY });

  /** The salary control's validators and disabled state are declarative, keyed off `salaryType`. */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.status, { message: "Status is required." });
      required(p.salaryType, { message: "Salary type is required." });
      required(p.salary, { message: "Salary is required." });
      min(p.salary, 0, { message: "Salary cannot be negative." });
      max(p.salary, 100, {
        when: ({ valueOf }) => valueOf(p.salaryType) === "share_of_gross",
        message: "Share of gross cannot exceed 100%.",
      });
      disabled(p.salary, { when: ({ valueOf }) => valueOf(p.salaryType) === "none" });
    },
    {
      submission: {
        action: async () => {
          const emp = this.employee();
          if (!emp?.id) {
            return undefined;
          }

          const v = this.model();
          const salaryType = v.salaryType;

          const command: UpdateEmployeeCommand = {
            userId: emp.id,
            // "none" salary type carries no salary: reactive forms excluded the disabled control
            // from the payload, so send undefined to preserve that behavior.
            salary:
              salaryType === "none"
                ? undefined
                : salaryType === "share_of_gross"
                  ? NumberUtils.toRatio(v.salary)
                  : v.salary,
            salaryType,
            status: v.status,
            address: v.address ?? undefined,
          };

          await this.api.invoke(updateEmployee, {
            userId: emp.id,
            body: command,
          });
          this.saved.emit();
          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const emp = this.employee();
      if (emp && this.visible()) {
        this.populateForm(emp);
        this.evaluateCanChangeRole(emp);
      }
    });
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
    return this.model().salaryType === "share_of_gross";
  }

  isNoneSalary(): boolean {
    return this.model().salaryType === "none";
  }

  private populateForm(emp: EmployeeDto): void {
    const salaryType = emp.salaryType ?? "none";
    const salary = emp.salary ?? 0;

    this.model.set({
      salary: salaryType === "share_of_gross" ? NumberUtils.toPercent(salary) : salary,
      salaryType,
      status: emp.status ?? "active",
      address: emp.address ?? null,
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
