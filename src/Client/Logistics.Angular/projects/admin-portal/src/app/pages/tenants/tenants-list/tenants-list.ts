import { Component, computed, inject, signal, viewChild } from "@angular/core";
import { Router } from "@angular/router";
import { Api, deleteTenant, resendTenantWelcome } from "@logistics/shared/api";
import {
  ConfirmDeleteDialog,
  DataContainer,
  PageHeader,
  SearchInput,
} from "@logistics/shared/components";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { Menu, MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { TenantsListStore } from "../store/tenants-list.store";

@Component({
  selector: "adm-tenants-list",
  templateUrl: "./tenants-list.html",
  providers: [TenantsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
    ConfirmDeleteDialog,
  ],
})
export class TenantsList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(TenantsListStore);

  private readonly actionMenu = viewChild<Menu>("actionMenu");
  private readonly selectedTenant = signal<{ id: string; name: string } | null>(null);

  protected readonly resendingTenantId = signal<string | null>(null);
  protected readonly deleteDialogVisible = signal(false);
  protected readonly deletingTenantId = signal<string | null>(null);
  protected readonly deletingTenantName = signal<string>("");
  protected readonly isDeleting = signal(false);

  protected readonly actionMenuItems = computed<MenuItem[]>(() => {
    const tenant = this.selectedTenant();
    return [
      {
        label: "Edit",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigate(["/tenants", tenant!.id, "edit"]),
      },
      {
        label: "Resend Welcome Email",
        icon: "pi pi-envelope",
        command: () => this.resendWelcome(tenant!.id),
      },
      { separator: true },
      {
        label: "Delete",
        icon: "pi pi-trash",
        styleClass: "text-red-600",
        command: () => this.openDeleteDialog(tenant!.id, tenant!.name),
      },
    ];
  });

  protected openActionMenu(event: Event, tenant: { id: string; name: string }): void {
    this.selectedTenant.set(tenant);
    this.actionMenu()?.toggle(event);
  }

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addTenant(): void {
    this.router.navigate(["/tenants/add"]);
  }

  protected openDeleteDialog(id: string, name: string): void {
    this.deletingTenantId.set(id);
    this.deletingTenantName.set(name);
    this.deleteDialogVisible.set(true);
  }

  protected async confirmDelete(): Promise<void> {
    this.isDeleting.set(true);
    try {
      await this.api.invoke(deleteTenant, { id: this.deletingTenantId()! });
      this.toastService.showSuccess("The tenant has been deleted successfully");
      this.store.removeItem(this.deletingTenantId()!);
      this.deleteDialogVisible.set(false);
    } finally {
      this.isDeleting.set(false);
    }
  }

  protected async resendWelcome(tenantId: string): Promise<void> {
    this.resendingTenantId.set(tenantId);
    try {
      await this.api.invoke(resendTenantWelcome, { id: tenantId });
      this.toastService.showSuccess("Welcome email has been resent successfully");
    } catch {
      this.toastService.showError("Failed to resend welcome email");
    } finally {
      this.resendingTenantId.set(null);
    }
  }

  protected getSubscriptionStatus(tenant: { subscription?: { status?: string } }): string {
    return tenant.subscription?.status ?? "none";
  }

  protected getStatusSeverity(
    status: string,
  ): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
    switch (status) {
      case "active":
        return "success";
      case "trialing":
        return "info";
      case "past_due":
        return "warn";
      case "cancelled":
      case "unpaid":
        return "danger";
      default:
        return "secondary";
    }
  }
}
