import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { form, FormField } from "@angular/forms/signals";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  deleteTenant,
  getTenantById,
  updateTenant,
  type TenantDto,
  type UpdateTenantCommand,
} from "@logistics/shared/api";
import { Card, PageHeader, Skeleton, Stack, Typography, UiButton } from "@logistics/shared/ui";
import { TenantForm, type TenantFormValue } from "@/shared/components";

@Component({
  selector: "adm-tenant-edit",
  templateUrl: "./tenant-edit.html",
  imports: [
    Card,
    FormField,
    PageHeader,
    RouterModule,
    Skeleton,
    Stack,
    TenantForm,
    Typography,
    UiButton,
  ],
})
export class TenantEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly isSavingAiSettings = signal(false);
  protected readonly tenant = signal<TenantDto | null>(null);

  protected readonly aiSettingsModel = signal({
    aiEnabled: true,
  });

  protected readonly aiSettingsForm = form(this.aiSettingsModel);

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
    this.aiSettingsModel.set({
      aiEnabled: tenant.settings?.aiEnabled !== false,
    });
    this.isFetching.set(false);
  }

  protected readonly initialValue = computed<Partial<TenantFormValue> | null>(() => {
    const tenant = this.tenant();
    if (!tenant) return null;

    return {
      name: tenant.name ?? "",
      companyName: tenant.companyName ?? "",
      billingEmail: tenant.billingEmail ?? "",
      dotNumber: tenant.dotNumber ?? "",
      companyAddress: tenant.companyAddress,
      region: tenant.settings?.region ?? "us",
      operatingMode: tenant.settings?.operatingMode ?? "fleet",
    };
  });

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
      companyAddress: formValue.companyAddress,
      settings: {
        ...tenant.settings,
        region: formValue.region,
        operatingMode: formValue.operatingMode,
      },
    };

    await this.api.invoke(updateTenant, { id: tenant.id!, body: command });
    this.toastService.showSuccess("Tenant has been updated successfully");
    this.router.navigateByUrl("/tenants");

    this.isLoading.set(false);
  }

  protected async saveAiSettings(): Promise<void> {
    const tenant = this.tenant();
    if (!tenant) return;

    this.isSavingAiSettings.set(true);
    try {
      const settings = this.aiSettingsForm().value();
      const command: UpdateTenantCommand = {
        id: tenant.id!,
        settings: {
          ...tenant.settings,
          aiEnabled: settings.aiEnabled,
        },
      };

      await this.api.invoke(updateTenant, { id: tenant.id!, body: command });
      this.toastService.showSuccess("AI settings updated");
    } catch {
      this.toastService.showError("Failed to update AI settings");
    } finally {
      this.isSavingAiSettings.set(false);
    }
  }

  protected async onRemove(): Promise<void> {
    const tenant = this.tenant();
    if (!tenant) return;

    await this.api.invoke(deleteTenant, { id: tenant.id! });
    this.toastService.showSuccess("Tenant has been deleted successfully");
    this.router.navigateByUrl("/tenants");
  }
}
