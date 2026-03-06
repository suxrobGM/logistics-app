import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteTenant } from "@logistics/shared/api";
import { ConfirmDeleteDialog, DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
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
    RouterLink,
    CardModule,
    TableModule,
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

  protected readonly deleteDialogVisible = signal(false);
  protected readonly deletingTenantId = signal<string | null>(null);
  protected readonly deletingTenantName = signal<string>("");
  protected readonly isDeleting = signal(false);

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

  protected getSubscriptionStatus(tenant: { subscription?: { status?: string } }): string {
    return tenant.subscription?.status ?? "none";
  }

  protected getStatusSeverity(status: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
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
