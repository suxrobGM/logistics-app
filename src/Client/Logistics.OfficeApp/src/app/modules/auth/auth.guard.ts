import { Injectable } from '@angular/core';
import { 
  ActivatedRouteSnapshot, 
  CanActivate, 
  Router, 
  RouterStateSnapshot, 
  UrlTree 
} from '@angular/router';
import { UserIdentity } from '@shared/models';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, Observable } from 'rxjs';

@Injectable()
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
    let user: UserIdentity | null;
    this.oidcService.userData$.subscribe(({userData}) => user = userData);

    return this.oidcService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => {
        const permission = route.data['permission'] as string;
        const hasAccess = user?.permission?.includes(permission);
        
        if (isAuthenticated && hasAccess) {
          return true;
        }

        return this.router.parseUrl('/unauthorized');
      })
    );
  }
}
