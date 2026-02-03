import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import type { CanActivateFn } from "@angular/router";
import { Router } from "@angular/router";
import { UserRole } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "./auth.service";

// Only app roles can access Admin Portal
const ALLOWED_ROLES = [UserRole.AppSuperAdmin, UserRole.AppAdmin, UserRole.AppManager];

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  // Wait for auth to be initialized before checking
  return toObservable(authService.authInitialized).pipe(
    filter((initialized) => initialized),
    take(1),
    switchMap(() => authService.onAuthenticated()),
    take(1),
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return router.parseUrl("/");
      }

      // Block tenant roles from Admin Portal - only app roles allowed
      const userData = authService.getUserData();
      const userRole = userData?.role;

      if (!userRole || !ALLOWED_ROLES.includes(userRole as UserRole)) {
        return router.parseUrl("/unauthorized");
      }

      // Check permission if specified in route data
      const requiredPermission = route.data["permission"] as string | undefined;
      if (requiredPermission && !permissionService.hasPermission(requiredPermission)) {
        return router.parseUrl("/unauthorized");
      }

      return true;
    }),
  );
};
