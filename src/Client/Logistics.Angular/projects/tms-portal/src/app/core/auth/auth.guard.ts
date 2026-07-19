import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { UserRole } from "@logistics/shared";
import { createRoleGuard } from "@logistics/shared/auth";
import { TenantService } from "../services";

// Only tenant roles can access TMS Portal (NOT app roles). After role and route-permission
// checks pass, the extra check enforces an active subscription.
export const authGuard = createRoleGuard({
  allowedRoles: [UserRole.Owner, UserRole.Manager, UserRole.Dispatcher, UserRole.Driver],
  unauthenticatedRedirect: "/unauthorized",
  extraCheck: () =>
    inject(TenantService).isSubscriptionActive()
      ? true
      : inject(Router).parseUrl("/unauthorized?reason=subscription"),
});
