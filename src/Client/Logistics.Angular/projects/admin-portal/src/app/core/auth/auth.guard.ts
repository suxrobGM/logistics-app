import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import type { CanActivateFn } from "@angular/router";
import { Router } from "@angular/router";
import { PermissionService } from "@logistics/shared/services";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "./auth.service";

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

      // Check permission if specified in route data
      const requiredPermission = route.data["permission"] as string | undefined;
      if (requiredPermission && !permissionService.hasPermission(requiredPermission)) {
        return router.parseUrl("/unauthorized");
      }

      return true;
    }),
  );
};
