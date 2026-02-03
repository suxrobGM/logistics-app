import { inject } from "@angular/core";
import { type CanActivateFn, Router } from "@angular/router";
import type { TenantFeature } from "../api/generated/models/tenant-feature";
import { FeatureService } from "../services/feature.service";

/**
 * Creates a route guard that checks if a feature is enabled for the current tenant.
 * If the feature is disabled, redirects to the unauthorized page.
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

    // Wait for features to be loaded before checking
    await featureService.waitForLoad();

    if (featureService.isEnabled(feature)) {
      return true;
    }

    return router.parseUrl("/unauthorized?reason=feature");
  };
}

/**
 * Route guard that reads the feature from route data.
 * The route must have a `feature` property in its data object.
 */
export const featureGuardFromData: CanActivateFn = async (route) => {
  const featureService = inject(FeatureService);
  const router = inject(Router);

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

  return router.parseUrl("/unauthorized?reason=feature");
};
