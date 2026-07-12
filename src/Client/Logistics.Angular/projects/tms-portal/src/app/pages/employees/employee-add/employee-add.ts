import { Component, DestroyRef, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import {
  Api,
  createEmployee,
  type CreateEmployeeCommand,
  type RoleDto,
  type SalaryType,
  type UserDto,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import {
  Container,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import {
  CurrencyField,
  PageHeader,
  UiAutocompleteField,
  UiFormField,
  UiSelectField,
  UnitField,
  ValidatedForm,
} from "@/shared/components";
import { UserService } from "../services";

interface EmployeeAddModel {
  user: UserDto | null;
  role: RoleDto | null;
  salary: number;
  salaryType: SalaryType;
}

const EMPTY: EmployeeAddModel = {
  user: null,
  role: null,
  salary: 0,
  salaryType: "none",
};

@Component({
  selector: "app-employee-add",
  templateUrl: "./employee-add.html",
  imports: [
    Container,
    CurrencyField,
    FormField,
    FormRoot,
    Icon,
    PageHeader,
    RouterLink,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiAutocompleteField,
    UiButton,
    UiFormField,
    UiSelectField,
    UnitField,
    ValidatedForm,
  ],
})
export class EmployeeAdd {
  protected readonly salaryTypes = salaryTypeOptions;

  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly userService = inject(UserService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly suggestedUsers = signal<UserDto[]>([]);
  protected readonly roles = signal<RoleDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);

  protected readonly model = signal<EmployeeAddModel>({ ...EMPTY });

  /** `isLoading` is retained separately because it tracks the initial roles load, not the submit. */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.user, { message: "Select a user." });
      required(p.salaryType, { message: "Salary type is required." });
      required(p.salary, { message: "Salary is required." });
      min(p.salary, 0, { message: "Salary cannot be negative." });
      max(p.salary, 100, {
        when: ({ valueOf }) => valueOf(p.salaryType) === "share_of_gross",
        message: "Share of gross cannot exceed 100%.",
      });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const user = v.user!;

          const newEmployee: CreateEmployeeCommand = {
            userId: user.id ?? undefined,
            role: v.role?.name,
            salary: v.salary,
            salaryType: v.salaryType,
          };

          await this.api.invoke(createEmployee, { body: newEmployee });
          this.toastService.showSuccess("New employee has been added successfully");
          this.form().reset({ ...EMPTY });
          return undefined;
        },
      },
    },
  );

  constructor() {
    this.fetchRoles();
  }

  searchUser(event: { query: string }): void {
    this.userService
      .searchUser(event.query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((users) => {
        if (users) {
          this.suggestedUsers.set(users);
        }
      });
  }

  clearSelectedRole(): void {
    this.model.update((m) => ({ ...m, role: null }));
  }

  isShareOfGrossSalary(): boolean {
    return this.model().salaryType === "share_of_gross";
  }

  isNoneSalary(): boolean {
    return this.model().salaryType === "none";
  }

  private fetchRoles(): void {
    this.isLoading.set(true);

    this.userService
      .fetchRoles()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((roles) => {
        if (roles) {
          this.roles.set(roles);
        }

        this.isLoading.set(false);
      });
  }
}
