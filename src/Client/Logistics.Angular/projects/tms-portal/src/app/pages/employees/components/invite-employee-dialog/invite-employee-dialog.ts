import { Component, inject, model, output, signal } from "@angular/core";
import { email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { UserRole } from "@logistics/shared";
import {
  Api,
  createInvitation,
  getTenantRoles,
  type CreateInvitationCommand,
  type RoleDto,
} from "@logistics/shared/api";
import {
  Spinner,
  Stack,
  UiButton,
  UiDialog,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { UiFormField } from "@/shared/components";

const EMPTY = { email: "", role: "", personalMessage: "" };

@Component({
  selector: "app-invite-employee-dialog",
  templateUrl: "./invite-employee-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Spinner,
    Stack,
    UiButton,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class InviteEmployeeDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly visible = model<boolean>(false);
  public readonly invitationSent = output<void>();

  protected readonly roles = signal<RoleDto[]>([]);

  private userRole?: string | null;

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.email, { message: "Email address is required." });
      email(p.email, { message: "Enter a valid email address." });
      required(p.role, { message: "Role is required." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          const command: CreateInvitationCommand = {
            email: v.email || null,
            type: "employee",
            tenantRole: v.role || null,
            personalMessage: v.personalMessage || null,
          };

          try {
            await this.api.invoke(createInvitation, { body: command });
            this.toastService.showSuccess(`Invitation sent to ${v.email}`);
            this.invitationSent.emit();
            this.close();
          } catch {
            this.toastService.showError("Failed to send invitation");
          }
          return undefined;
        },
      },
    },
  );

  constructor() {
    const user = this.authService.getUserData();
    this.userRole = user?.role;
    this.fetchRoles();
  }

  close(): void {
    this.visible.set(false);
    this.form().reset({ ...EMPTY });
  }

  private async fetchRoles(): Promise<void> {
    try {
      const result = await this.api.invoke(getTenantRoles, {});
      if (result.items) {
        let roles = [...result.items];

        // Filter out Customer role - customers are invited via customer dialog
        roles = roles.filter((r) => r.name !== UserRole.Customer);

        // Owner role can only be assigned by SuperAdmin/Admin
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
