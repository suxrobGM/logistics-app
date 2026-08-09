import type { TenantFeature } from "@logistics/shared/api";
import type { FeatureService, PermissionService } from "@logistics/shared/services";

/** The two gates any permission-and-feature-gated thing declares: nav items, dashboard panels. */
export interface AccessGate {
  permission?: string;
  /** One flag, or several of which **any** enabled one satisfies the gate. */
  feature?: TenantFeature | TenantFeature[];
}

/**
 * Feature requirement satisfied **and** permission held. Shared by the sidebar, the settings tab
 * bar and the dashboard panel registry so they can't disagree. Role filtering stays out.
 */
export function passesAccessGate(
  gate: AccessGate,
  featureService: FeatureService,
  permissionService: PermissionService,
): boolean {
  if (gate.permission && !permissionService.hasPermission(gate.permission)) {
    return false;
  }
  return featureService.isAnyEnabled(gate.feature);
}
