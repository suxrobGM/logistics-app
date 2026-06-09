import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ToastService } from "@logistics/shared";
import {
  Api,
  getDefaultFeatures,
  updateDefaultFeatures,
  type DefaultFeatureStatusDto,
  type TenantFeature,
} from "@logistics/shared/api";
import { FeatureRow, PageHeader } from "@logistics/shared/components";
import { FEATURE_DESCRIPTIONS } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToggleSwitchModule } from "primeng/toggleswitch";

@Component({
  selector: "adm-default-features",
  templateUrl: "./default-features.html",
  imports: [
    FormsModule,
    CardModule,
    ProgressSpinnerModule,
    ButtonModule,
    ToggleSwitchModule,
    FeatureRow,
    PageHeader,
  ],
})
export class DefaultFeatures implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly features = signal<DefaultFeatureStatusDto[]>([]);

  ngOnInit(): void {
    this.loadFeatures();
  }

  private async loadFeatures(): Promise<void> {
    this.isLoading.set(true);
    try {
      const features = await this.api.invoke(getDefaultFeatures);
      this.features.set(features ?? []);
    } catch {
      this.toastService.showError("Failed to load default feature settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async saveAll(): Promise<void> {
    this.isSaving.set(true);
    try {
      const updates = this.features().map((f) => ({
        feature: f.feature,
        isEnabledByDefault: f.isEnabledByDefault,
      }));

      await this.api.invoke(updateDefaultFeatures, {
        body: { features: updates },
      });
      this.toastService.showSuccess("Default feature settings have been saved");
    } catch {
      this.toastService.showError("Failed to save default feature settings");
    } finally {
      this.isSaving.set(false);
    }
  }

  protected toggleFeature(feature: DefaultFeatureStatusDto): void {
    this.features.update((features) =>
      features.map((f) =>
        f.feature === feature.feature ? { ...f, isEnabledByDefault: !f.isEnabledByDefault } : f,
      ),
    );
  }

  protected getFeatureDescription(feature: TenantFeature | undefined): string {
    if (!feature) return "";
    return FEATURE_DESCRIPTIONS[feature] ?? "";
  }
}
