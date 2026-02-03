import { Injectable, effect, inject, signal } from "@angular/core";
import { Api, getCurrentTenantFeatures } from "@logistics/shared/api";
import type { FeatureStatusDto } from "@logistics/shared/api";
import { type FeatureProvider } from "@logistics/shared/services";
import { TenantService } from "./tenant.service";

/**
 * TMS Portal implementation of FeatureProvider.
 * Fetches and caches feature configurations for the current tenant.
 */
@Injectable({ providedIn: "root" })
export class TmsFeatureProvider implements FeatureProvider {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  private readonly features = signal<FeatureStatusDto[]>([]);
  private loaded = false;
  private loadPromise: Promise<void> | null = null;

  constructor() {
    // Refresh features when tenant data changes
    effect(() => {
      this.tenantService.tenantData();
      if (this.tenantService.getTenantId()) {
        this.refreshFeatures();
      }
    });
  }

  getFeatures(): FeatureStatusDto[] {
    return this.features();
  }

  isLoaded(): boolean {
    return this.loaded;
  }

  async waitForLoad(): Promise<void> {
    if (this.loaded) {
      return;
    }

    if (this.loadPromise) {
      return this.loadPromise;
    }

    // If not loading and not loaded, trigger a load
    if (this.tenantService.getTenantId()) {
      await this.refreshFeatures();
    }
  }

  async refreshFeatures(): Promise<void> {
    if (!this.tenantService.getTenantId()) {
      return;
    }

    // Prevent concurrent loads
    if (this.loadPromise) {
      return this.loadPromise;
    }

    this.loadPromise = this.doRefresh();
    try {
      await this.loadPromise;
    } finally {
      this.loadPromise = null;
    }
  }

  private async doRefresh(): Promise<void> {
    try {
      const result = await this.api.invoke(getCurrentTenantFeatures, {});
      if (result) {
        this.features.set(result);
        this.loaded = true;
      }
    } catch (error) {
      console.error("Failed to fetch tenant features:", error);
      // Mark as loaded even on error to prevent infinite retries
      this.loaded = true;
    }
  }
}
