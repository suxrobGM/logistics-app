import { inject } from "@angular/core";
import type { CanActivateFn } from "@angular/router";
import { Router } from "@angular/router";
import { TenantContextService } from "../services";

/**
 * Guard that ensures user has selected a tenant before accessing protected routes.
 * If user has multiple tenants and none is selected, redirects to tenant selection page.
 */
export const tenantGuard: CanActivateFn = async () => {
  const router = inject(Router);
  const tenantService = inject(TenantContextService);

  // Make sure tenants are loaded
  await tenantService.loadTenants();

  // If user has no tenants, redirect to select-tenant page to show "No Access" message
  if (tenantService.availableTenants().length === 0) {
    console.warn("User has no tenant access");
    router.navigate(["/select-tenant"]);
    return false;
  }

  // If single tenant, it's auto-selected
  if (tenantService.hasSingleTenant()) {
    return true;
  }

  // If multiple tenants and one is selected, proceed
  if (tenantService.currentTenant()) {
    return true;
  }

  // Need to select tenant
  router.navigate(["/select-tenant"]);
  return false;
};
