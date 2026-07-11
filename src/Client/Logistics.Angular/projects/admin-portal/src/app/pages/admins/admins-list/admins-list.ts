import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import {
  addAdmin,
  Api,
  cancelAdminInvitation,
  resendAdminInvitation,
  revokeAdmin,
  type InvitationDto,
  type UserDto,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  DataContainer,
  PageHeader,
  SearchField,
  UiButton,
  UiDataTable,
  UiDialog,
  UiSortHeader,
  UiTextField,
  UiTooltip,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { AdminInvitationsListStore } from "../store/admin-invitations-list.store";
import { AdminsListStore } from "../store/admins-list.store";

@Component({
  selector: "adm-admins-list",
  templateUrl: "./admins-list.html",
  providers: [AdminsListStore, AdminInvitationsListStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    DatePipe,
    PageHeader,
    SearchField,
    UiButton,
    UiDataTable,
    UiDialog,
    UiSortHeader,
    UiTextField,
    UiTooltip,
  ],
})
export class AdminsList {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);
  protected readonly store = inject(AdminsListStore);
  protected readonly invitationsStore = inject(AdminInvitationsListStore);

  protected readonly showAddDialog = signal(false);
  protected readonly addEmail = signal("");
  protected readonly addMessage = signal("");
  protected readonly isAdding = signal(false);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected getFullName(user: UserDto): string {
    return `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim() || "—";
  }

  protected getRoleSeverity(role?: string | null): UiBadgeIntent {
    return role === "Super Admin" ? "danger" : "info";
  }

  protected isSuperAdmin(user: UserDto): boolean {
    return user.role === "Super Admin";
  }

  // ─── Add admin (grant existing user or invite by email) ───
  protected openAddDialog(): void {
    this.addEmail.set("");
    this.addMessage.set("");
    this.showAddDialog.set(true);
  }

  protected async submitAddAdmin(): Promise<void> {
    const email = this.addEmail().trim();
    if (!email) {
      this.toast.showError("Please enter an email address");
      return;
    }

    this.isAdding.set(true);
    try {
      const message = this.addMessage().trim();
      const result = await this.api.invoke(addAdmin, {
        body: { email, personalMessage: message || undefined },
      });

      this.showAddDialog.set(false);

      if (result.invited) {
        this.toast.showSuccess(`Invitation sent to ${email}`);
        this.invitationsStore.load();
      } else {
        this.toast.showSuccess(`${email} is now an admin`);
        this.store.load();
      }
    } catch {
      this.toast.showError(
        "Could not add admin. They may already be an admin or have a pending invitation.",
      );
    } finally {
      this.isAdding.set(false);
    }
  }

  // ─── Revoke admin ───
  protected revoke(admin: UserDto): void {
    this.toast.confirm({
      header: "Revoke admin access",
      message: `Are you sure you want to revoke admin access from ${admin.email}?`,
      icon: "warning",
      severity: "danger",
      accept: async () => {
        try {
          await this.api.invoke(revokeAdmin, { userId: admin.id! });
          this.toast.showSuccess(`Revoked admin access from ${admin.email}`);
          this.store.removeItem(admin.id!);
        } catch {
          this.toast.showError("Failed to revoke admin access");
        }
      },
    });
  }

  // ─── Pending invitations ───
  protected async resendInvitation(invitation: InvitationDto): Promise<void> {
    if (!invitation.id) return;
    try {
      await this.api.invoke(resendAdminInvitation, { id: invitation.id });
      this.toast.showSuccess("Invitation resent");
    } catch {
      this.toast.showError("Failed to resend invitation");
    }
  }

  protected cancelInvitation(invitation: InvitationDto): void {
    const id = invitation.id;
    if (!id) return;
    this.toast.confirm({
      header: "Cancel invitation",
      message: `Cancel the admin invitation for ${invitation.email}?`,
      icon: "warning",
      severity: "danger",
      accept: async () => {
        try {
          await this.api.invoke(cancelAdminInvitation, { id });
          this.toast.showSuccess("Invitation cancelled");
          this.invitationsStore.removeItem(id);
        } catch {
          this.toast.showError("Failed to cancel invitation");
        }
      },
    });
  }
}
