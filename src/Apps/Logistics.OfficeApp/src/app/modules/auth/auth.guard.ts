import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { User } from '@app/shared/models/user';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private oidcSecurityService: OidcSecurityService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree 
  {
    let user: User;
    this.oidcSecurityService.userData$.subscribe(({userData}) => user = userData);

    return this.oidcSecurityService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => {
        const roles = route.data['roles'];
        
        if (isAuthenticated && roles && roles.indexOf(user.role) !== -1) {
          return true;
        }

        return this.router.parseUrl('/unauthorized');
      })
    );
  }
}
