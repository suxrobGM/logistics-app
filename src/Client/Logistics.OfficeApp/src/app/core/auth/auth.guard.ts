import { inject } from "@angular/core";
import { type CanActivateFn, Router } from "@angular/router";
import { map } from "rxjs";
import { AuthService, UserData } from "@/core/auth";
import { TenantService } from "../services";

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const tenantService = inject(TenantService);

  let user: UserData | null;
  authService.onUserDataChanged().subscribe((userData) => (user = userData));

  return authService.onAuthenticated().pipe(
    map((isAuthenticated) => {
      const permission = route.data["permission"] as string;
      const hasAccess = !permission || user?.permissions?.includes(permission);

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
