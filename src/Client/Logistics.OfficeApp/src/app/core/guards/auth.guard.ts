import {inject} from "@angular/core";
import { CanActivateFn, Router} from "@angular/router";
import {map} from "rxjs";
import {AuthService, UserData} from "@/core/auth";

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  let user: UserData | null;
  authService.onUserDataChanged().subscribe((userData) => (user = userData));

  return authService.onAuthenticated().pipe(
    map((isAuthenticated) => {
      const permission = route.data["permission"] as string;
      const hasAccess = user?.permissions?.includes(permission);

      if (isAuthenticated && hasAccess) {
        return true;
      }

      return router.parseUrl("/unauthorized");
    })
  );
}
