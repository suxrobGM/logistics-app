import { Injectable, inject } from "@angular/core";
import { DEFAULT_TENANT_SETTINGS, type TenantSettings } from "@logistics/shared";
import { type TenantSettingsProvider } from "@logistics/shared/services";
import { TenantService } from "./tenant.service";

/**
 * TMS Portal implementation of TenantSettingsProvider.
 * Wraps TenantService to provide tenant localization settings.
 */
@Injectable({ providedIn: "root" })
export class TmsTenantSettingsProvider implements TenantSettingsProvider {
  private readonly tenantService = inject(TenantService);

  getSettings(): TenantSettings {
    const settings = this.tenantService.getTenantData()?.settings;
    return settings == null ? DEFAULT_TENANT_SETTINGS : settings;
  }
}
