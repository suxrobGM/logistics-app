import { Injectable, inject } from "@angular/core";
import { DEFAULT_TENANT_SETTINGS, type TenantSettings } from "@logistics/shared";
import type { TenantSettingsProvider } from "@logistics/shared/services";
import { TenantContextService } from "./tenant-context.service";

/**
 * Customer Portal implementation of TenantSettingsProvider.
 * Provides tenant localization settings from the current tenant context.
 */
@Injectable({ providedIn: "root" })
export class CustomerPortalSettingsProvider implements TenantSettingsProvider {
  private readonly tenantContext = inject(TenantContextService);

  getSettings(): TenantSettings {
    const settings = this.tenantContext.currentTenant()?.settings;
    return settings == null ? DEFAULT_TENANT_SETTINGS : settings;
  }
}
