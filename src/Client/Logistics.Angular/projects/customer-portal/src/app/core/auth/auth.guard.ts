import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { type CanActivateFn, Router } from "@angular/router";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "./auth.service";

/**
 * Guard that checks if the user is authenticated.
 * Allows users with tenant.customer role immediately.
 * For CustomerUser (no role), tenantGuard will verify Portal.Access permission after tenant is set.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for auth initialization to complete before checking
  return toObservable(authService.authInitialized).pipe(
    filter((initialized) => initialized),
    take(1),
    switchMap(() => authService.onAuthenticated()),
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return router.parseUrl("/login");
      }

      // Allow all authenticated users to proceed
      // - Users with tenant.customer role are allowed
      // - CustomerUser (no role) will be verified in tenantGuard via Portal.Access permission
      // - Users without any tenant access will be redirected in tenantGuard
      return true;
    }),
  );
};
