import { computed, inject, Injectable, signal } from "@angular/core";
import { EventTypes, OidcSecurityService, PublicEventsService } from "angular-auth-oidc-client";
import { filter, from, map, Observable, switchMap } from "rxjs";
import { UserData, userRoleOptions } from "../models";
import { PermissionService } from "../services";
import { AUTH_HOOKS } from "./auth-hooks";
import { AUTH_OPTIONS } from "./auth-options";

/**
 * Single shared authentication service for every portal. It is the superset of the three
 * portal-specific services that used to exist: admin's signal-based user data, the common
 * OIDC event/observable surface, and the tenant/permission wiring - the latter two made
 * portal-configurable via {@link AUTH_HOOKS} and {@link AUTH_OPTIONS}.
 */
@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly oidcService = inject(OidcSecurityService);
  private readonly eventService = inject(PublicEventsService);
  private readonly permissionService = inject(PermissionService);
  private readonly hooks = inject(AUTH_HOOKS, { optional: true });
  private readonly options = inject(AUTH_OPTIONS, { optional: true });

  private readonly _userData = signal<UserData | null>(null);
  private readonly _authInitialized = signal(false);

  /**
   * Signal containing the current user data.
   */
  public readonly userData = this._userData.asReadonly();

  /**
   * Signal containing the user's display name, or `null` when unknown.
   * Portals that want a branded fallback ("Admin") apply it at the call site.
   */
  public readonly userName = computed(() => this._userData()?.name ?? null);

  /**
   * Signal indicating whether the initial auth check has completed.
   * This includes loading user data and (when enabled) permissions.
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
      this._userData.set(null);
      this.permissionService.clearPermissions();
      this.hooks?.onLoggedOut?.();
    });
  }

  /**
   * Initiate the authentication process and check if the user is authenticated.
   * If the user is authenticated, set the user data, run the `onAuthenticated` hook, and
   * (when enabled) load permissions.
   * @returns An observable that emits a boolean value indicating whether the user is authenticated
   */
  checkAuth(): Observable<boolean> {
    return this.oidcService.checkAuth().pipe(
      switchMap((response) => {
        if (response.isAuthenticated) {
          const userData = new UserData(response.userData);
          this._userData.set(userData);

          return from(Promise.resolve(this.hooks?.onAuthenticated?.(userData))).pipe(
            switchMap(() =>
              (this.options?.loadPermissionsOnInit ?? true)
                ? from(this.permissionService.loadPermissions())
                : from(Promise.resolve()),
            ),
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
