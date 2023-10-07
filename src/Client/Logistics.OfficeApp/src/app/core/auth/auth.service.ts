/* eslint-disable indent */
import {Injectable} from '@angular/core';
import {EventTypes, OidcSecurityService, PublicEventsService} from 'angular-auth-oidc-client';
import {Observable, filter, map} from 'rxjs';
import {UserRoleHelper} from '@core/helpers';
import {UserData} from './userData';


@Injectable({providedIn: 'root'})
export class AuthService {
  private accessToken: string | null;
  private userData: UserData | null;

  constructor(
    private oidcService: OidcSecurityService,
    private eventService: PublicEventsService)
  {
    this.accessToken = null;
    this.userData = null;
    this.onUserDataChanged().subscribe((_) => _);
  }

  onAuthenticated(): Observable<boolean> {
    return this.oidcService.isAuthenticated$.pipe(map(({isAuthenticated}) => isAuthenticated));
  }

  onUserDataChanged(): Observable<UserData | null> {
    return this.oidcService.userData$.pipe(map(({userData}) => {
      if (userData) {
        this.userData = new UserData(userData);
      }

      return this.userData;
    }));
  }

  onCheckingAuth(): Observable<void> {
    return this.eventService.registerForEvents()
        .pipe(filter((notifaction) => notifaction.type === EventTypes.CheckingAuth), map(() => void 0));
  }

  onCheckingAuthFinished(): Observable<void> {
    return this.eventService.registerForEvents()
        .pipe(filter((notifaction) => notifaction.type === EventTypes.CheckingAuthFinished), map(() => void 0));
  }

  login() {
    this.oidcService.authorize();
  }

  logout() {
    this.oidcService.logoff().subscribe(() => {
      this.userData = null;
      this.accessToken = null;
    });
  }

  checkAuth(): Observable<boolean> {
    return this.oidcService.checkAuth().pipe(
      map(({isAuthenticated, userData, accessToken}) => {
        console.log(userData);

        if (isAuthenticated) {
          this.userData = new UserData(userData);
        }

        if (accessToken) {
          this.accessToken = accessToken;
        }
        return isAuthenticated;
      }),
    );
  }

  getAccessToken(): Observable<string> {
    return this.oidcService.getAccessToken();
  }

  getUserData(): UserData | null {
    return this.userData;
  }

  getUserRoleName(): string | null {
    return UserRoleHelper.getRoleName(this.userData?.roles[0]);
  }
}
