import { inject, Injector, runInInjectionContext } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { Router, type CanActivateFn, type UrlTree } from "@angular/router";
import { filter, map, switchMap, take } from "rxjs";
import { UserRole } from "../models";
import { PermissionService } from "../services";
import { AuthService } from "./auth.service";

/**
 * Configuration for {@link createRoleGuard}. Every portal builds its route guard from this
 * factory; the differences between portals collapse to the fields below.
 */
export interface RoleGuardOptions {
  /** When set, only these roles may pass; any other role is redirected as unauthorized. */
  allowedRoles?: UserRole[];
  /** Where to send an unauthenticated user. */
  unauthenticatedRedirect: string;
  /** Where to send an authenticated-but-forbidden user. Defaults to `/unauthorized`. */
  unauthorizedRedirect?: string;
  /** Whether to enforce the `route.data["permission"]` check. Defaults to `true`. */
  checkRoutePermission?: boolean;
  /** Extra check run last (in the guard's injection context) when everything else passes. */
  extraCheck?: () => true | UrlTree;
}

/**
 * Build a `CanActivateFn` that waits for auth initialization, then applies the configured
 * role / route-permission / extra checks. All dependencies are injected synchronously at the
 * start of the guard so the RxJS callbacks never run outside the injection context.
 */
export function createRoleGuard(options: RoleGuardOptions): CanActivateFn {
  return (route) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const permissionService = inject(PermissionService);
    const injector = inject(Injector);

    const unauthorizedRedirect = options.unauthorizedRedirect ?? "/unauthorized";

    // Wait for auth initialization to complete before checking.
    return toObservable(auth.authInitialized, { injector }).pipe(
      filter(Boolean),
      take(1),
      switchMap(() => auth.onAuthenticated()),
      take(1),
      map((isAuthenticated) => {
        if (!isAuthenticated) {
          return router.parseUrl(options.unauthenticatedRedirect);
        }

        if (options.allowedRoles) {
          const userRole = auth.getUserData()?.role;
          if (!userRole || !options.allowedRoles.includes(userRole as UserRole)) {
            return router.parseUrl(unauthorizedRedirect);
          }
        }

        if (options.checkRoutePermission !== false) {
          const permission = route.data["permission"] as string | undefined;
          if (permission && !permissionService.hasPermission(permission)) {
            return router.parseUrl(unauthorizedRedirect);
          }
        }

        if (options.extraCheck) {
          return runInInjectionContext(injector, options.extraCheck);
        }

        return true;
      }),
    );
  };
}
