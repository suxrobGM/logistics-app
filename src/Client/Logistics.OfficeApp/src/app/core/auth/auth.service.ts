import {Injectable} from "@angular/core";
import {EventTypes, OidcSecurityService, PublicEventsService} from "angular-auth-oidc-client";
import {Observable, filter, map} from "rxjs";
import {UserRoleEnum} from "@/core/enums";
import {TenantService} from "@/core/services";
import {UserData} from "./user-data";

@Injectable({providedIn: "root"})
export class AuthService {
  private userData: UserData | null = null;

  constructor(
    private readonly oidcService: OidcSecurityService,
    private readonly eventService: PublicEventsService,
    private readonly tenantService: TenantService
  ) {
    this.onUserDataChanged().subscribe((_) => _);
  }

  onAuthenticated(): Observable<boolean> {
    return this.oidcService.isAuthenticated$.pipe(map(({isAuthenticated}) => isAuthenticated));
  }

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

    return UserRoleEnum.getValue(roleValue).description;
  }
}
