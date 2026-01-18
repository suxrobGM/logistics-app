import { Injectable, inject, signal } from "@angular/core";
import { UserData } from "@logistics/shared";
import { EventTypes, OidcSecurityService, PublicEventsService } from "angular-auth-oidc-client";
import { Observable, filter, from, map, switchMap } from "rxjs";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly oidcService = inject(OidcSecurityService);
  private readonly eventService = inject(PublicEventsService);

  private userData: UserData | null = null;
  private readonly _authInitialized = signal(false);

  /**
   * Signal indicating whether the initial auth check has completed.
   */
  public readonly authInitialized = this._authInitialized.asReadonly();

  /**
   * Register for the event that is emitted when the user is authenticated
   */
  onAuthenticated(): Observable<boolean> {
    return this.oidcService.isAuthenticated$.pipe(map(({ isAuthenticated }) => isAuthenticated));
  }

  /**
   * Register for the event that is emitted when the user's data is changed
   */
  onUserDataChanged(): Observable<UserData | null> {
    return this.oidcService.userData$.pipe(
      map(({ userData }) => {
        if (userData) {
          this.userData = new UserData(userData);
        }
        return this.userData;
      }),
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is started
   */
  onCheckingAuth(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notification) => notification.type === EventTypes.CheckingAuth),
      map(() => void 0),
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is finished
   */
  onCheckingAuthFinished(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notification) => notification.type === EventTypes.CheckingAuthFinished),
      map(() => void 0),
    );
  }

  login(): void {
    this.oidcService.authorize();
  }

  logout(): void {
    this.oidcService.logoff().subscribe(() => {
      this.userData = null;
    });
  }

  /**
   * Initiate the authentication process and check if the user is authenticated
   */
  checkAuth(): Observable<boolean> {
    return this.oidcService.checkAuth().pipe(
      switchMap((response) => {
        if (response.isAuthenticated) {
          this.userData = new UserData(response.userData);
          this._authInitialized.set(true);
          return from(Promise.resolve(true));
        }

        this._authInitialized.set(true);
        return from(Promise.resolve(false));
      }),
    );
  }

  getAccessToken(): Observable<string> {
    return this.oidcService.getAccessToken();
  }

  getUserData(): UserData | null {
    return this.userData;
  }
}
