import { Component, inject, input, model, output, signal } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { UiSelectField } from "@logistics/shared";
import {
  Api,
  updateEmployee,
  type RoleDto,
  type UpdateEmployeeCommand,
} from "@logistics/shared/api";
import { Spinner, UiButton, UiDialog } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { UserService } from "../../services";

const EMPTY = { role: "" };

@Component({
  selector: "app-change-role-dialog",
  templateUrl: "./change-role-dialog.html",
  imports: [FormField, FormRoot, Spinner, UiButton, UiDialog, UiSelectField],
})
export class ChangeRoleDialog {
  private readonly api = inject(Api);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);

  public readonly userId = input.required<string>();
  public readonly currentRole = input<RoleDto | null>(null);
  public readonly visible = model<boolean>(false);
  public readonly roleChanged = output<void>();

  protected readonly roles = signal<RoleDto[]>([]);
  protected readonly isLoading = signal(false);

  protected readonly model = signal({ ...EMPTY });

  /**
   * An empty `role` on submit runs `onInvalid`, which shows the "select a role" toast — this dialog
   * has no inline `ui-form-field` error slot to surface the error otherwise.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.role, { message: "Select a role from the list" });
    },
    {
      submission: {
        action: async () => {
          const command: UpdateEmployeeCommand = {
            userId: this.userId(),
            role: this.model().role,
          };

          await this.api.invoke(updateEmployee, {
            userId: this.userId(),
            body: command,
          });
          this.toastService.showSuccess(`Successfully changed employee's role`);
          this.roleChanged.emit();
          return undefined;
        },
        onInvalid: () => {
          this.toastService.showError("Select a role from the list");
        },
      },
    },
  );

  constructor() {
    this.fetchRoles();
  }

  close(): void {
    this.visible.set(false);
    this.clearSelectedRole();
  }

  clearSelectedRole(): void {
    this.model.set({ ...EMPTY });
  }

  async removeRole(): Promise<void> {
    const command: UpdateEmployeeCommand = {
      userId: this.userId(),
      role: undefined,
    };

    this.isLoading.set(true);
    await this.api.invoke(updateEmployee, {
      userId: this.userId(),
      body: command,
    });
    this.toastService.showSuccess("Removed role from the employee");
    this.roleChanged.emit();
    this.isLoading.set(false);
  }

  private fetchRoles(): void {
    this.userService.fetchRoles().subscribe((roles) => {
      if (roles) {
        this.roles.set(roles);
      }
    });
  }
}
