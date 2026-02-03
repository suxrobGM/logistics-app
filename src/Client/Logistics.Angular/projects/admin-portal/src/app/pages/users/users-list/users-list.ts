import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { PasswordModule } from "primeng/password";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ImpersonationService, ToastService } from "@/core/services";
import { UsersListStore } from "../store/users-list.store";

@Component({
  selector: "adm-users-list",
  templateUrl: "./users-list.html",
  providers: [UsersListStore],
  imports: [
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    PasswordModule,
    FormsModule,
  ],
})
export class UsersList {
  protected readonly store = inject(UsersListStore);
  private readonly impersonationService = inject(ImpersonationService);
  private readonly toast = inject(ToastService);

  protected readonly showImpersonateDialog = signal(false);
  protected readonly impersonateEmail = signal("");
  protected readonly masterPassword = signal("");
  protected readonly isImpersonating = signal(false);

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

  protected getRoleSeverity(
    role?: string | null,
  ): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
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
