import { Injectable, InjectionToken, computed, inject, signal } from "@angular/core";
import type { FeatureStatusDto } from "../api/generated/models/feature-status-dto";
import type { TenantFeature } from "../api/generated/models/tenant-feature";

/**
 * Interface for providing tenant feature data to the feature service.
 * Applications should implement this and provide it via the FEATURE_PROVIDER token.
 */
export interface FeatureProvider {
  /**
   * Get all feature statuses for the current tenant.
   */
  getFeatures(): FeatureStatusDto[];

  /**
   * Whether features have been loaded from the server.
   */
  isLoaded(): boolean;

  /**
   * Refresh features from the server.
   */
  refreshFeatures(): Promise<void>;

  /**
   * Wait for features to be loaded (for use in guards).
   */
  waitForLoad(): Promise<void>;
}

/**
 * Injection token for the feature provider.
 * Applications must provide an implementation of FeatureProvider.
 */
export const FEATURE_PROVIDER = new InjectionToken<FeatureProvider>("FeatureProvider");

/**
 * Service for checking if features are enabled for the current tenant.
 * Used throughout the application to conditionally show/hide UI elements.
 */
@Injectable({ providedIn: "root" })
export class FeatureService {
  private readonly provider = inject(FEATURE_PROVIDER, { optional: true });

  /**
   * Check if a specific feature is enabled for the current tenant.
   * @param feature The feature to check
   * @returns true if enabled, false otherwise
   */
  isEnabled(feature: TenantFeature): boolean {
    const features = this.provider?.getFeatures() ?? [];
    const status = features.find((f) => f.feature === feature);
    return status?.isEnabled ?? true; // Default to enabled if not found
  }

  /**
   * Check if a specific feature is locked by admin.
   * @param feature The feature to check
   * @returns true if admin-locked, false otherwise
   */
  isLocked(feature: TenantFeature): boolean {
    const features = this.provider?.getFeatures() ?? [];
    const status = features.find((f) => f.feature === feature);
    return status?.isAdminLocked ?? false;
  }

  /**
   * Get all feature statuses.
   */
  getAllFeatures(): FeatureStatusDto[] {
    return this.provider?.getFeatures() ?? [];
  }

  /**
   * Refresh features from the server.
   */
  async refreshFeatures(): Promise<void> {
    await this.provider?.refreshFeatures();
  }

  /**
   * Check if features have been loaded from the server.
   */
  isLoaded(): boolean {
    return this.provider?.isLoaded() ?? false;
  }

  /**
   * Wait for features to be loaded (for use in guards).
   */
  async waitForLoad(): Promise<void> {
    await this.provider?.waitForLoad();
  }
}

/**
 * Human-readable descriptions for each feature.
 * Used in feature settings UI.
 */
export const FEATURE_DESCRIPTIONS: Record<TenantFeature, string> = {
  dashboard: "View company dashboard and analytics",
  employees: "Manage employees, drivers, and staff",
  loads: "Manage loads and shipments",
  trucks: "Manage fleet vehicles and trucks",
  customers: "Manage customer directory",
  invoices: "Create and manage load invoices",
  payments: "Process and track payments",
  eld: "Electronic Logging Device (HOS) compliance",
  load_board: "Access load boards and find freight",
  messages: "Internal messaging system",
  notifications: "Push notifications and alerts",
  safety: "Safety compliance and DVIR reports",
  expenses: "Track and manage expenses",
  payroll: "Manage payroll and employee payments",
  timesheets: "Track employee time and attendance",
  maintenance: "Fleet maintenance scheduling",
  trips: "Trip planning and management",
  reports: "Generate and view reports",
};
