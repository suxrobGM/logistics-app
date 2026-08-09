import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { Permission } from "@logistics/shared";
import {
  Api,
  getCurrentTenantFeatures,
  updateCurrentTenantFeature,
  type FeatureStatusDto,
  type TenantFeature,
} from "@logistics/shared/api";
import { FEATURE_DESCRIPTIONS } from "@logistics/shared/services";
import {
  Card,
  Container,
  FeatureRow,
  Icon,
  Spinner,
  Stack,
  Typography,
  UiToggleField,
} from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { ReadOnlyNotice } from "../components";

@Component({
  selector: "app-feature-settings",
  templateUrl: "./feature-settings.html",
  imports: [
    Card,
    Container,
    FeatureRow,
    Icon,
    PageHeader,
    ReadOnlyNotice,
    Spinner,
    Stack,
    Typography,
    UiToggleField,
  ],
})
export class FeatureSettingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly permissionService = inject(PermissionService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal<TenantFeature | null>(null);
  protected readonly features = signal<FeatureStatusDto[]>([]);

  /** Manager holds `Tenant.View` and reaches this tab, but every toggle needs `Tenant.Manage`. */
  protected readonly managePermission = Permission.Tenant.Manage;
  protected readonly canManage = computed(() =>
    this.permissionService.hasPermission(this.managePermission),
  );

  ngOnInit(): void {
    this.loadFeatures();
  }

  private async loadFeatures(): Promise<void> {
    this.isLoading.set(true);
    try {
      const features = await this.api.invoke(getCurrentTenantFeatures);
      this.features.set(features ?? []);
    } catch {
      this.toastService.showError("Failed to load feature settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async toggleFeature(feature: FeatureStatusDto): Promise<void> {
    if (!this.canManage()) return;

    if (feature.isAdminLocked) {
      this.toastService.showWarning(
        "This feature is locked by the administrator and cannot be changed",
      );
      return;
    }

    if (!feature.feature) return;

    this.isSaving.set(feature.feature);
    try {
      await this.api.invoke(updateCurrentTenantFeature, {
        feature: feature.feature,
        body: { isEnabled: !feature.isEnabled },
      });

      // Update local state
      this.features.update((features) =>
        features.map((f) =>
          f.feature === feature.feature ? { ...f, isEnabled: !f.isEnabled } : f,
        ),
      );

      this.toastService.showSuccess(
        `${feature.name} has been ${!feature.isEnabled ? "enabled" : "disabled"}`,
      );
    } catch {
      this.toastService.showError(`Failed to update ${feature.name}`);
    } finally {
      this.isSaving.set(null);
    }
  }

  protected getFeatureDescription(feature: TenantFeature | undefined): string {
    if (!feature) return "";
    return FEATURE_DESCRIPTIONS[feature] ?? "";
  }
}
