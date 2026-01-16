import { Component, inject, input, model, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Api, removeRoleFromEmployee, updateEmployee } from "@logistics/shared/api";
import type {
  RemoveRoleFromEmployeeCommand,
  RoleDto,
  UpdateEmployeeCommand,
} from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToastService } from "@/core/services";
import { UserService } from "../../services";

@Component({
  selector: "app-change-role-dialog",
  templateUrl: "./change-role-dialog.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
  ],
})
export class ChangeRoleDialogComponent {
  protected readonly form: FormGroup;

  private readonly api = inject(Api);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);

  public readonly userId = input.required<string>();
  public readonly currentRoles = input<RoleDto[]>([]);
  public readonly visible = model<boolean>(false);

  protected readonly roles = signal<RoleDto[]>([]);
  protected readonly isLoading = signal(false);

  constructor() {
    this.form = new FormGroup({
      role: new FormControl("", Validators.required),
    });

    this.fetchRoles();
  }

  async submit(): Promise<void> {
    const role = this.form.value.role;

    if (role === "") {
      this.toastService.showError("Select a role from the list");
      return;
    }

    const command: UpdateEmployeeCommand = {
      userId: this.userId(),
      role: role,
    };

    this.isLoading.set(true);
    await this.api.invoke(updateEmployee, {
      userId: this.userId(),
      body: command,
    });
    this.toastService.showSuccess(`Successfully changed employee's role`);

    this.isLoading.set(false);
  }

  close(): void {
    this.visible.set(false);
    //this.visibleChange.emit(false);
    this.clearSelectedRole();
  }

  clearSelectedRole(): void {
    this.form.patchValue({
      role: { name: "", displayName: " " },
    });
  }

  removeRoles(): void {
    this.currentRoles()?.forEach((role) => {
      if (role.name) {
        this.removeRole(role.name);
      }
    });
  }

  private async removeRole(roleName: string): Promise<void> {
    const command: RemoveRoleFromEmployeeCommand = {
      userId: this.userId(),
      role: roleName,
    };

    this.isLoading.set(true);
    await this.api.invoke(removeRoleFromEmployee, {
      userId: this.userId(),
      body: command,
    });
    this.toastService.showSuccess(`Removed ${roleName} role from the employee`);

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
