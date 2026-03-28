import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { FormField, form } from "@angular/forms/signals";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { LabeledField, ToastService } from "@logistics/shared";
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
  imports: [
    CardModule,
    ButtonModule,
    RouterModule,
    FormField,
    TenantForm,
    DividerModule,
    SkeletonModule,
    LabeledField,
  ],
})
export class TenantEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly isSavingLlmSettings = signal(false);
  protected readonly tenant = signal<TenantDto | null>(null);

  protected readonly llmSettingsModel = signal({
    llmModel: "" as string,
    enableExtendedThinking: false,
  });

  protected readonly llmSettingsForm = form(this.llmSettingsModel);

  protected readonly aiModelOptions = [
    { label: "Claude Sonnet 4.6", value: "claude-sonnet-4-6" },
    { label: "Claude Haiku 4.5", value: "claude-haiku-4-5" },
    { label: "Claude Opus 4.6", value: "claude-opus-4-6" },
  ];

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
    this.llmSettingsModel.set({
      llmModel: tenant.settings?.llmModel ?? "",
      enableExtendedThinking: tenant.settings?.llmExtendedThinking ?? false,
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
    };

    await this.api.invoke(updateTenant, { id: tenant.id!, body: command });
    this.toastService.showSuccess("Tenant has been updated successfully");
    this.router.navigateByUrl("/tenants");

    this.isLoading.set(false);
  }

  protected async saveLlmSettings(): Promise<void> {
    const tenant = this.tenant();
    if (!tenant) return;

    this.isSavingLlmSettings.set(true);
    try {
      const settings = this.llmSettingsForm().value();
      const command: UpdateTenantCommand = {
        id: tenant.id!,
        settings: {
          ...tenant.settings,
          llmModel: settings.llmModel,
          llmExtendedThinking: settings.enableExtendedThinking,
        },
      };

      await this.api.invoke(updateTenant, { id: tenant.id!, body: command });
      this.toastService.showSuccess("LLM settings updated");
    } catch {
      this.toastService.showError("Failed to update LLM settings");
    } finally {
      this.isSavingLlmSettings.set(false);
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
