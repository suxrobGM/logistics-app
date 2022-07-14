import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  isAuthenticated: boolean;

  constructor(private oidcSecurityService: OidcSecurityService, private router: Router) {
    this.isAuthenticated = false;
    
    this.oidcSecurityService.isAuthenticated().subscribe((isAuthenticated) => {
      this.isAuthenticated = isAuthenticated;
    })
  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree 
  {
    // if (!this.isAuthenticated) {
    //   this.oidcSecurityService.authorize();
    //   return false;
    // }

    return this.oidcSecurityService.isAuthenticated();
  }
}
