import { Injectable, computed, inject, signal } from "@angular/core";
import { PermissionService } from "@logistics/shared";
import { UserData, userRoleOptions } from "@logistics/shared";
import { EventTypes, OidcSecurityService, PublicEventsService } from "angular-auth-oidc-client";
import { Observable, filter, from, map, switchMap } from "rxjs";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly oidcService = inject(OidcSecurityService);
  private readonly eventService = inject(PublicEventsService);
  private readonly permissionService = inject(PermissionService);

  private readonly _userData = signal<UserData | null>(null);
  private readonly _authInitialized = signal(false);

  /**
   * Signal containing the current user data.
   */
  public readonly userData = this._userData.asReadonly();

  /**
   * Signal containing the user's display name.
   */
  public readonly userName = computed(() => this._userData()?.name ?? "Admin");

  /**
   * Signal indicating whether the initial auth check has completed.
   * This includes loading user data and permissions.
   */
  public readonly authInitialized = this._authInitialized.asReadonly();

  /**
   * Register for the event that is emitted when the user is authenticated
   * @returns An observable that emits a boolean value indicating whether the user is authenticated
   */
  onAuthenticated(): Observable<boolean> {
    return this.oidcService.isAuthenticated$.pipe(map(({ isAuthenticated }) => isAuthenticated));
  }

  /**
   * Register for the event that is emitted when the user's data is changed
   * @returns An observable that emits the user
   */
  onUserDataChanged(): Observable<UserData | null> {
    return this.oidcService.userData$.pipe(
      map(({ userData }) => {
        if (userData) {
          this._userData.set(new UserData(userData));
        }

        return this._userData();
      }),
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is started
   */
  onCheckingAuth(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notifaction) => notifaction.type === EventTypes.CheckingAuth),
      map(() => void 0),
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is finished
   */
  onCheckingAuthFinished(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notifaction) => notifaction.type === EventTypes.CheckingAuthFinished),
      map(() => void 0),
    );
  }

  login(): void {
    this.oidcService.authorize();
  }

  logout(): void {
    this.oidcService.logoff().subscribe(() => {
      this._userData.set(null);
      this.permissionService.clearPermissions();
    });
  }

  /**
   * Initiate the authentication process and check if the user is authenticated
   * If the user is authenticated, set the user data and load permissions
   * @returns An observable that emits a boolean value indicating whether the user is authenticated
   */
  checkAuth(): Observable<boolean> {
    return this.oidcService.checkAuth().pipe(
      switchMap((response) => {
        if (response.isAuthenticated) {
          this._userData.set(new UserData(response.userData));

          return from(this.permissionService.loadPermissions()).pipe(
            map(() => {
              this._authInitialized.set(true);
              return true;
            }),
          );
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
    return this._userData();
  }

  getUserRoleName(): string | null {
    const roleValue = this._userData()?.role;

    if (!roleValue) {
      return null;
    }

    const roleDesc = userRoleOptions.find((option) => option.value === roleValue);
    return roleDesc?.label ?? null;
  }
}
