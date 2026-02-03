import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { type CanActivateFn, Router } from "@angular/router";
import { Permission, UserRole } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "./auth.service";

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const permissionService = inject(PermissionService);

  // Wait for auth initialization to complete before checking
  return toObservable(authService.authInitialized).pipe(
    filter((initialized) => initialized),
    take(1),
    switchMap(() => authService.onAuthenticated()),
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return router.parseUrl("/login");
      }

      // Allow tenant.customer role OR users with Portal.Access permission (CustomerUser)
      const userData = authService.getUserData();
      const userRole = userData?.role;
      const hasPortalPermission = permissionService.hasPermission(Permission.Portal.Access);

      if (userRole !== UserRole.Customer && !hasPortalPermission) {
        return router.parseUrl("/unauthorized");
      }

      return true;
    }),
  );
};
