import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { type CanActivateFn, Router } from "@angular/router";
import { UserRole } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "@/core/auth";
import { TenantService } from "../services";

// Only tenant roles can access TMS Portal (NOT app roles)
const ALLOWED_ROLES = [UserRole.Owner, UserRole.Manager, UserRole.Dispatcher, UserRole.Driver];

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const tenantService = inject(TenantService);
  const permissionService = inject(PermissionService);

  // Wait for auth initialization to complete before checking permissions
  return toObservable(authService.authInitialized).pipe(
    filter((initialized) => initialized),
    take(1),
    switchMap(() => authService.onAuthenticated()),
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return router.parseUrl("/unauthorized");
      }

      // Block app roles from TMS Portal - only tenant roles allowed
      const userData = authService.getUserData();
      const userRole = userData?.role;

      if (!userRole || !ALLOWED_ROLES.includes(userRole as UserRole)) {
        return router.parseUrl("/unauthorized");
      }

      const permission = route.data["permission"] as string;
      const hasAccess = !permission || permissionService.hasPermission(permission);

      if (!hasAccess) {
        return router.parseUrl("/unauthorized");
      }

      // Check if the user has an active subscription
      if (!tenantService.isSubscriptionActive()) {
        return router.parseUrl("/unauthorized?reason=subscription");
      }

      return true;
    }),
  );
};
