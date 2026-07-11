import { Component, inject, signal, viewChild } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Badge,
  Card,
  DataContainer,
  PageHeader,
  SearchField,
  UiButton,
  UiDataTable,
  UiDialog,
  UiMenu,
  UiPasswordField,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { ImpersonationService, ToastService } from "@/core/services";
import { UsersListStore } from "../store/users-list.store";

@Component({
  selector: "adm-users-list",
  templateUrl: "./users-list.html",
  providers: [UsersListStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    PageHeader,
    RouterLink,
    SearchField,
    UiButton,
    UiDataTable,
    UiDialog,
    UiMenu,
    UiPasswordField,
    UiSortHeader,
  ],
})
export class UsersList {
  protected readonly store = inject(UsersListStore);
  private readonly impersonationService = inject(ImpersonationService);
  private readonly toast = inject(ToastService);

  private readonly actionMenu = viewChild<UiMenu>("actionMenu");
  private readonly selectedEmail = signal("");

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "Impersonate User",
      icon: "user",
      command: () => this.openImpersonateDialog(this.selectedEmail()),
    },
  ];

  protected readonly showImpersonateDialog = signal(false);
  protected readonly impersonateEmail = signal("");
  protected readonly masterPassword = signal("");
  protected readonly isImpersonating = signal(false);

  protected openActionMenu(event: Event, user: { email?: string | null }): void {
    this.selectedEmail.set(user.email ?? "");
    this.actionMenu()?.toggle(event);
  }

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected openImpersonateDialog(email: string): void {
    this.impersonateEmail.set(email);
    this.masterPassword.set("");
    this.showImpersonateDialog.set(true);
  }

  protected async impersonate(): Promise<void> {
    if (!this.masterPassword()) {
      this.toast.showError("Please enter the master password");
      return;
    }

    this.isImpersonating.set(true);
    try {
      const result = await this.impersonationService.impersonate({
        targetEmail: this.impersonateEmail(),
        masterPassword: this.masterPassword(),
      });

      this.showImpersonateDialog.set(false);
      this.toast.showSuccess(`Opening session as ${this.impersonateEmail()}`);

      // Open impersonation URL in new tab
      window.open(result.impersonationUrl!, "_blank");
    } catch {
      this.toast.showError("Failed to impersonate user. Please check the master password.");
    } finally {
      this.isImpersonating.set(false);
    }
  }

  protected getRoleSeverity(role?: string | null): UiBadgeIntent {
    switch (role?.toLowerCase()) {
      case "superadmin":
        return "danger";
      case "owner":
        return "warn";
      case "manager":
        return "info";
      case "dispatcher":
        return "secondary";
      default:
        return "secondary";
    }
  }

  protected getFullName(user: { firstName?: string | null; lastName?: string | null }): string {
    const first = user.firstName ?? "";
    const last = user.lastName ?? "";
    return `${first} ${last}`.trim() || "N/A";
  }
}
