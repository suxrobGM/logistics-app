import { inject } from "@angular/core";
import type { CanActivateFn } from "@angular/router";
import { Router } from "@angular/router";
import { Permission } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { TenantContextService } from "../services";

/**
 * Guard that ensures user has selected a tenant and has portal access permission.
 * If user has multiple tenants and none is selected, redirects to tenant selection page.
 * After tenant is set, loads and verifies portal access permissions.
 */
export const tenantGuard: CanActivateFn = async () => {
  const router = inject(Router);
  const tenantService = inject(TenantContextService);
  const permissionService = inject(PermissionService);

  // Load tenants (cached after first call)
  await tenantService.loadTenants();

  // No tenant access - show "No Access" message
  if (tenantService.availableTenants().length === 0) {
    router.navigate(["/select-tenant"]);
    return false;
  }

  // Multiple tenants but none selected - need to pick one
  if (tenantService.hasMultipleTenants() && !tenantService.currentTenant()) {
    router.navigate(["/select-tenant"]);
    return false;
  }

  // Tenant is set (single auto-selected or manually selected) - verify permissions
  await permissionService.loadPermissions();

  if (!permissionService.hasPermission(Permission.Portal.Access)) {
    router.navigate(["/unauthorized"]);
    return false;
  }

  return true;
};
