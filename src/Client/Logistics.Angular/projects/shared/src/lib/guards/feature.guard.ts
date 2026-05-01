import { inject } from "@angular/core";
import { Router, UrlTree, type CanActivateFn } from "@angular/router";
import type { TenantFeature } from "../api/generated/models/tenant-feature";
import { UPGRADE_HANDLER, type IUpgradePromptHandler } from "../errors/upgrade-handler";
import { FeatureService } from "../services/feature.service";

/**
 * Creates a route guard that checks if a feature is enabled for the current tenant.
 * If the feature is not in the plan, shows an upgrade dialog.
 * If the feature is admin-locked, redirects to the unauthorized page.
 *
 * Usage in routes:
 * ```typescript
 * {
 *   path: "eld",
 *   loadChildren: () => import("./pages/eld/eld.routes"),
 *   canActivate: [featureGuard("eld")],
 * }
 * ```
 *
 * Or using route data:
 * ```typescript
 * {
 *   path: "eld",
 *   loadChildren: () => import("./pages/eld/eld.routes"),
 *   canActivate: [featureGuardFromData],
 *   data: { feature: "eld" },
 * }
 * ```
 */
export function featureGuard(feature: TenantFeature): CanActivateFn {
  return async () => {
    const featureService = inject(FeatureService);
    const router = inject(Router);
    const upgradeHandler = inject(UPGRADE_HANDLER, { optional: true });

    // Wait for features to be loaded before checking
    await featureService.waitForLoad();

    if (featureService.isEnabled(feature)) {
      return true;
    }

    return handleDisabledFeature(featureService, router, upgradeHandler, feature);
  };
}

/**
 * Route guard that reads the feature from route data.
 * The route must have a `feature` property in its data object.
 */
export const featureGuardFromData: CanActivateFn = async (route) => {
  const featureService = inject(FeatureService);
  const router = inject(Router);
  const upgradeHandler = inject(UPGRADE_HANDLER, { optional: true });

  const feature = route.data["feature"] as TenantFeature | undefined;

  // If no feature specified, allow access
  if (!feature) {
    return true;
  }

  // Wait for features to be loaded before checking
  await featureService.waitForLoad();

  if (featureService.isEnabled(feature)) {
    return true;
  }

  return handleDisabledFeature(featureService, router, upgradeHandler, feature);
};

/**
 * Handles the case when a feature is disabled.
 * Shows upgrade dialog if not in plan, otherwise redirects to unauthorized.
 */
function handleDisabledFeature(
  featureService: FeatureService,
  router: Router,
  upgradeHandler: IUpgradePromptHandler | null,
  feature: TenantFeature,
): boolean | UrlTree {
  const allFeatures = featureService.getAllFeatures();
  const status = allFeatures.find((f) => f.feature === feature);

  // If the feature is not in the plan (and not admin-locked), show upgrade dialog
  if (status && !status.isIncludedInPlan && !status.isAdminLocked && upgradeHandler) {
    const featureName = status.name ?? feature;
    upgradeHandler.showUpgradePrompt(
      "FEATURE_NOT_IN_PLAN",
      `The '${featureName}' feature is not included in your current subscription plan. Please upgrade to access this feature.`,
    );
    // Block navigation - the dialog will show as an overlay on the current page
    return false;
  }

  // Admin locked or no upgrade handler - redirect to unauthorized
  return router.parseUrl("/unauthorized?reason=feature");
}
