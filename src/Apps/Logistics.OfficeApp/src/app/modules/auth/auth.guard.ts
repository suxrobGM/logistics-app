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

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree 
  {
    let user: User | null;
    this.oidcService.userData$.subscribe(({userData}) => user = userData);

    return this.oidcService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => {
        let hasAccess = false;
        const roles = route.data['roles'] as string[];

        for (const role of roles) {
          hasAccess = user?.role?.includes(role) ?? false;

          if (hasAccess) {
            break;
          }
        }
        
        if (isAuthenticated && hasAccess) {
          return true;
        }

        return this.router.parseUrl('/unauthorized');
      })
    );
  }
}
