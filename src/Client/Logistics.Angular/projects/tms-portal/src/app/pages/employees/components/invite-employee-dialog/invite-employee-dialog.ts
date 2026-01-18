import { Component, inject, model, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { UserRole } from "@logistics/shared";
import { Api, createInvitation, getTenantRoles } from "@logistics/shared/api";
import type { CreateInvitationCommand, RoleDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { LabeledField } from "@/shared/components";

@Component({
  selector: "app-invite-employee-dialog",
  templateUrl: "./invite-employee-dialog.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    LabeledField,
  ],
})
export class InviteEmployeeDialogComponent {
  protected readonly form = new FormGroup({
    email: new FormControl("", [Validators.required, Validators.email]),
    role: new FormControl("", Validators.required),
    personalMessage: new FormControl(""),
  });

  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly visible = model<boolean>(false);
  public readonly invitationSent = output<void>();

  protected readonly roles = signal<RoleDto[]>([]);
  protected readonly isLoading = signal(false);

  private userRole?: string | null;

  constructor() {
    const user = this.authService.getUserData();
    this.userRole = user?.role;
    this.fetchRoles();
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.toastService.showError("Please fill in all required fields");
      return;
    }

    const formValue = this.form.value;

    const command: CreateInvitationCommand = {
      email: formValue.email ?? null,
      type: "employee",
      tenantRole: formValue.role ?? null,
      personalMessage: formValue.personalMessage ?? null,
    };

    this.isLoading.set(true);
    try {
      await this.api.invoke(createInvitation, { body: command });
      this.toastService.showSuccess(`Invitation sent to ${formValue.email}`);
      this.invitationSent.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to send invitation");
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
    this.form.reset();
  }

  private async fetchRoles(): Promise<void> {
    try {
      const result = await this.api.invoke(getTenantRoles, {});
      if (result.items) {
        let roles = [...result.items];

        // Filter out Customer role - customers are invited via customer dialog
        roles = roles.filter((r) => r.name !== UserRole.Customer);

        // Owner role can only be assigned by SuperAdmin/AppManager
        const canAssignOwner =
          this.userRole === UserRole.AppSuperAdmin || this.userRole === UserRole.AppAdmin;

        if (!canAssignOwner) {
          roles = roles.filter((r) => r.name !== UserRole.Owner);
        }

        // Managers cannot assign Manager role
        if (this.userRole === UserRole.Manager) {
          roles = roles.filter((r) => r.name !== UserRole.Manager);
        }

        this.roles.set(roles);
      }
    } catch {
      this.toastService.showError("Failed to load roles");
    }
  }
}
