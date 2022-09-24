import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { User } from '@shared/models/user';
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

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree 
  {
    let user: User | null;
    this.oidcService.userData$.subscribe(({userData}) => user = userData);

    return this.oidcService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => {
        const roles = route.data['roles'];
        const hasRole = roles && roles.indexOf(user?.role) !== -1
        
        if (isAuthenticated && hasRole) {
          return true;
        }

        return this.router.parseUrl('/unauthorized');
      })
    );
  }
}
