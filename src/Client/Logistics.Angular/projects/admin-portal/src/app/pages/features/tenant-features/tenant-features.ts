import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, getTenantById, getTenantFeatures, updateTenantFeatures } from "@logistics/shared/api";
import type { FeatureStatusDto, TenantDto, TenantFeature } from "@logistics/shared/api";
import { PageHeader } from "@logistics/shared/components";
import { FEATURE_DESCRIPTIONS } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { TooltipModule } from "primeng/tooltip";

@Component({
  selector: "adm-tenant-features",
  templateUrl: "./tenant-features.html",
  imports: [
    FormsModule,
    CardModule,
    ProgressSpinnerModule,
    ButtonModule,
    TableModule,
    ToggleSwitchModule,
    TooltipModule,
    CheckboxModule,
    PageHeader,
  ],
})
export class TenantFeatures implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  public readonly tenantId = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly tenant = signal<TenantDto | null>(null);
  protected readonly features = signal<FeatureStatusDto[]>([]);

  ngOnInit(): void {
    this.loadData();
  }

  private async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [tenant, features] = await Promise.all([
        this.api.invoke(getTenantById, { identifier: this.tenantId() }),
        this.api.invoke(getTenantFeatures, { tenantId: this.tenantId() }),
      ]);
      this.tenant.set(tenant ?? null);
      this.features.set(features ?? []);
    } catch {
      this.toastService.showError("Failed to load tenant feature settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async saveAll(): Promise<void> {
    this.isSaving.set(true);
    try {
      const updates = this.features().map((f) => ({
        feature: f.feature,
        isEnabled: f.isEnabled,
        isAdminLocked: f.isAdminLocked,
      }));

      await this.api.invoke(updateTenantFeatures, {
        tenantId: this.tenantId(),
        body: { features: updates },
      });
      this.toastService.showSuccess("Feature settings have been saved");
    } catch {
      this.toastService.showError("Failed to save feature settings");
    } finally {
      this.isSaving.set(false);
    }
  }

  protected goBack(): void {
    this.router.navigate(["/tenants", this.tenantId(), "edit"]);
  }

  protected toggleEnabled(feature: FeatureStatusDto): void {
    this.features.update((features) =>
      features.map((f) => (f.feature === feature.feature ? { ...f, isEnabled: !f.isEnabled } : f)),
    );
  }

  protected toggleLocked(feature: FeatureStatusDto): void {
    this.features.update((features) =>
      features.map((f) =>
        f.feature === feature.feature ? { ...f, isAdminLocked: !f.isAdminLocked } : f,
      ),
    );
  }

  protected getFeatureDescription(feature: TenantFeature | undefined): string {
    if (!feature) return "";
    return FEATURE_DESCRIPTIONS[feature] ?? "";
  }
}
