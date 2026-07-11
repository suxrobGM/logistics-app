import { Component, inject, signal, type OnInit } from "@angular/core";
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
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";

@Component({
  selector: "app-feature-settings",
  templateUrl: "./feature-settings.html",
  imports: [
    Card,
    Container,
    FeatureRow,
    Icon,
    PageHeader,
    Spinner,
    Stack,
    Typography,
    UiToggleField,
  ],
})
export class FeatureSettingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal<TenantFeature | null>(null);
  protected readonly features = signal<FeatureStatusDto[]>([]);

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
