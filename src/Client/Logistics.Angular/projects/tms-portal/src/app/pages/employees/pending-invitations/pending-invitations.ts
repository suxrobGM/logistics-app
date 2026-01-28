import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { InviteEmployeeDialog } from "../components";
import { PendingInvitationsStore } from "../store";

@Component({
  selector: "app-pending-invitations",
  templateUrl: "./pending-invitations.html",
  providers: [PendingInvitationsStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    DatePipe,
    TagModule,
    DataContainer,
    PageHeader,
    SearchInput,
    InviteEmployeeDialog,
  ],
})
export class PendingInvitations {
  protected readonly store = inject(PendingInvitationsStore);
  private readonly toastService = inject(ToastService);

  protected readonly inviteDialogVisible = signal(false);

  openInviteDialog(): void {
    this.inviteDialogVisible.set(true);
  }

  onInvitationSent(): void {
    this.store.fetchData();
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  resendInvitation(id: string): void {
    this.toastService.confirm({
      message: "Are you sure you want to resend this invitation?",
      header: "Resend Invitation",
      icon: "pi pi-send",
      acceptLabel: "Resend",
      rejectLabel: "Cancel",
      accept: () => {
        this.store.resend(id);
      },
    });
  }

  cancelInvitation(id: string): void {
    this.toastService.confirm({
      message: "Are you sure you want to cancel this invitation? This action cannot be undone.",
      header: "Cancel Invitation",
      icon: "pi pi-exclamation-triangle",
      acceptLabel: "Cancel Invitation",
      rejectLabel: "Keep",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => {
        this.store.cancel(id);
      },
    });
  }

  getInvitationTypeSeverity(
    type: string,
  ): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" {
    return type === "Employee" ? "info" : "success";
  }

  isExpiringSoon(expiresAt: string): boolean {
    const expiry = new Date(expiresAt);
    const now = new Date();
    const hoursUntilExpiry = (expiry.getTime() - now.getTime()) / (1000 * 60 * 60);
    return hoursUntilExpiry < 24 && hoursUntilExpiry > 0;
  }
}
