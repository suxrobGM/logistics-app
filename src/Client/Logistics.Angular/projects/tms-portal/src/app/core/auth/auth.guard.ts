import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { type CanActivateFn, Router } from "@angular/router";
import { PermissionService } from "@logistics/shared/services";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "@/core/auth";
import { TenantService } from "../services";

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
      const permission = route.data["permission"] as string;
      const hasAccess = !permission || permissionService.hasPermission(permission);

      // Check if the user has an active subscription
      if (!tenantService.isSubscriptionActive()) {
        return router.parseUrl("/unauthorized?reason=subscription");
      }

      if (isAuthenticated && hasAccess) {
        return true;
      }

      return router.parseUrl("/unauthorized");
    }),
  );
};
