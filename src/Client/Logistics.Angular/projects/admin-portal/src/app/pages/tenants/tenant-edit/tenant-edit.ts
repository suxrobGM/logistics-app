import { Component, type OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, deleteTenant, getTenantById, updateTenant } from "@logistics/shared/api";
import type { TenantDto, UpdateTenantCommand } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { TenantForm, type TenantFormValue } from "@/shared/components";

@Component({
  selector: "adm-tenant-edit",
  templateUrl: "./tenant-edit.html",
  imports: [CardModule, ButtonModule, RouterModule, TenantForm, DividerModule, SkeletonModule],
})
export class TenantEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly tenant = signal<TenantDto | null>(null);

  ngOnInit(): void {
    this.fetchTenant();
  }

  private async fetchTenant(): Promise<void> {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      this.router.navigateByUrl("/tenants");
      return;
    }

    this.isFetching.set(true);
    const tenant = await this.api.invoke(getTenantById, { identifier: id });

    if (!tenant) {
      this.toastService.showError("Tenant not found");
      this.router.navigateByUrl("/tenants");
      return;
    }

    this.tenant.set(tenant);
    this.isFetching.set(false);
  }

  protected getInitialValue(): Partial<TenantFormValue> | null {
    const tenant = this.tenant();
    if (!tenant) return null;

    return {
      name: tenant.name ?? "",
      companyName: tenant.companyName ?? "",
      billingEmail: tenant.billingEmail ?? "",
      dotNumber: tenant.dotNumber ?? "",
      companyAddress: tenant.companyAddress,
    };
  }

  protected async onSave(formValue: TenantFormValue): Promise<void> {
    const tenant = this.tenant();
    if (!tenant) return;

    this.isLoading.set(true);

    const command: UpdateTenantCommand = {
      id: tenant.id!,
      name: formValue.name,
      companyName: formValue.companyName,
      billingEmail: formValue.billingEmail,
      dotNumber: formValue.dotNumber || undefined,
      companyAddress: formValue.companyAddress ?? undefined,
    };

    await this.api.invoke(updateTenant, { id: tenant.id!, body: command });
    this.toastService.showSuccess("Tenant has been updated successfully");
    this.router.navigateByUrl("/tenants");

    this.isLoading.set(false);
  }

  protected async onRemove(): Promise<void> {
    const tenant = this.tenant();
    if (!tenant) return;

    await this.api.invoke(deleteTenant, { id: tenant.id! });
    this.toastService.showSuccess("Tenant has been deleted successfully");
    this.router.navigateByUrl("/tenants");
  }
}
