import type { FeatureService, PermissionService } from "@logistics/shared/services";
import type { NavItem } from "./nav-menu.types";

/**
 * Feature requirement satisfied **and** permission held. Shared by `SidebarNavService` and the
 * settings tab bar so they can't disagree. Role filtering stays out - only the sidebar applies it.
 */
export function isNavItemVisible(
  item: NavItem,
  featureService: FeatureService,
  permissionService: PermissionService,
): boolean {
  if (item.permission && !permissionService.hasPermission(item.permission)) {
    return false;
  }
  return featureService.isAnyEnabled(item.feature);
}
