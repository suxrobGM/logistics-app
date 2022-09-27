import { Injectable } from '@angular/core';
import { 
  ActivatedRouteSnapshot, 
  CanActivate, 
  Router, 
  RouterStateSnapshot, 
  UrlTree 
} from '@angular/router';
import { User } from '@shared/models';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private oidcService: OidcSecurityService, 
    private router: Router)
  {
  }

  public canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree 
  {
    let user: User | null;
    this.oidcService.userData$.subscribe(({userData}) => user = userData);

    return this.oidcService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => {
        const allowedRoles = route.data['roles'] as string[];
        const hasAccess = this.checkRole(allowedRoles, user?.role);
        
        if (isAuthenticated && hasAccess) {
          return true;
        }

        return this.router.parseUrl('/unauthorized');
      })
    );
  }

  private checkRole(allowedRoles: string[], userRoles?: string | string[]): boolean {
    let hasRole = false;

    for (const role of allowedRoles) {
      hasRole = userRoles?.includes(role) ?? false;

      if (hasRole) {
        break;
      }
    }

    return hasRole;
  }
}
