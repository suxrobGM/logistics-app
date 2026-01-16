import { inject } from "@angular/core";
import { toObservable } from "@angular/core/rxjs-interop";
import { type CanActivateFn, Router } from "@angular/router";
import { filter, map, switchMap, take } from "rxjs";
import { AuthService } from "./auth.service";

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for auth initialization to complete before checking
  return toObservable(authService.authInitialized).pipe(
    filter((initialized) => initialized),
    take(1),
    switchMap(() => authService.onAuthenticated()),
    map((isAuthenticated) => {
      if (isAuthenticated) {
        return true;
      }

      return router.parseUrl("/login");
    }),
  );
};
