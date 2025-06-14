import { Injectable, inject } from "@angular/core";
import {EventTypes, OidcSecurityService, PublicEventsService} from "angular-auth-oidc-client";
import {Observable, filter, map} from "rxjs";
import {TenantService} from "@/core/services";
import {userRoleOptions} from "../../shared/models";
import {UserData} from "./user-data";

@Injectable({providedIn: "root"})
export class AuthService {
  private readonly oidcService = inject(OidcSecurityService);
  private readonly eventService = inject(PublicEventsService);
  private readonly tenantService = inject(TenantService);

  private userData: UserData | null = null;

  /**
   * Register for the event that is emitted when the user is authenticated
   * @returns An observable that emits a boolean value indicating whether the user is authenticated
   */
  onAuthenticated(): Observable<boolean> {
    return this.oidcService.isAuthenticated$.pipe(map(({isAuthenticated}) => isAuthenticated));
  }

  /**
   * Register for the event that is emitted when the user's data is changed
   * @returns An observable that emits the user
   */
  onUserDataChanged(): Observable<UserData | null> {
    return this.oidcService.userData$.pipe(
      map(({userData}) => {
        if (userData) {
          this.userData = new UserData(userData);
        }

        return this.userData;
      })
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is started
   */
  onCheckingAuth(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notifaction) => notifaction.type === EventTypes.CheckingAuth),
      map(() => void 0)
    );
  }

  /**
   * Register for the event that is emitted when the authentication process is finished
   */
  onCheckingAuthFinished(): Observable<void> {
    return this.eventService.registerForEvents().pipe(
      filter((notifaction) => notifaction.type === EventTypes.CheckingAuthFinished),
      map(() => void 0)
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
   * If the user is authenticated, set the user data and tenant ID
   * @returns An observable that emits a boolean value indicating whether the user is authenticated
   */
  checkAuth(): Observable<boolean> {
    return this.oidcService.checkAuth().pipe(
      map((response) => {
        if (response.isAuthenticated) {
          this.userData = new UserData(response.userData);
          this.tenantService.setTenantId(this.userData.tenant);
        }

        console.log("User data:", this.userData);
        //console.log("Access token:", response.accessToken);
        return response.isAuthenticated;
      })
    );
  }

  getAccessToken(): Observable<string> {
    return this.oidcService.getAccessToken();
  }

  getUserData(): UserData | null {
    return this.userData;
  }

  getUserRoleName(): string | null {
    const roleValue = this.userData?.roles[0];

    if (!roleValue) {
      return null;
    }

    const roleDesc = userRoleOptions.find((option) => option.value === roleValue);
    return roleDesc?.label ?? null;
  }
}
