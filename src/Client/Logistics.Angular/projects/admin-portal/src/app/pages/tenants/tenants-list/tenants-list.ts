import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteTenant } from "@logistics/shared/api";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
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
    ConfirmDialogModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
  ],
})
export class TenantsList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(TenantsListStore);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addTenant(): void {
    this.router.navigate(["/tenants/add"]);
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("tenant", () => this.deleteTenant(id));
  }

  private async deleteTenant(id: string): Promise<void> {
    await this.api.invoke(deleteTenant, { id });
    this.toastService.showSuccess("The tenant has been deleted successfully");
    this.store.removeItem(id);
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
