import {Injectable} from '@angular/core';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import {map, Observable} from 'rxjs';
import {AuthService, UserData} from '@core/auth';


@Injectable()
export class AuthGuard {
  constructor(
    private authService: AuthService,
    private router: Router)
  {
  }

  public canActivate(
      route: ActivatedRouteSnapshot,
      state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree
  {
    let user: UserData | null;
    this.authService.onUserDataChanged().subscribe((userData) => user = userData);

    return this.authService.onAuthenticated().pipe(
        map((isAuthenticated) => {
          const permission = route.data['permission'] as string;
          const hasAccess = user?.permissions?.includes(permission);

          if (isAuthenticated && hasAccess) {
            return true;
          }

          return this.router.parseUrl('/unauthorized');
        }),
    );
  }
}
