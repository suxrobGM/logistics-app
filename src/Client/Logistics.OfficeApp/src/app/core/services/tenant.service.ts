import {HttpHeaders} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {SubscriptionStatus, TenantDto} from "@/core/models";
import {CookieService} from "./cookie.service";
import {ApiService} from "./api.service";

@Injectable({providedIn: "root"})
export class TenantService {
  private tenantId: string | null = null;
  private tenantData: TenantDto | null = null;

  constructor(
    private readonly cookieService: CookieService,
    private readonly apiService: ApiService
  ) {}

  getTenantData(): TenantDto | null {
    return this.tenantData;
  }

  /**
   * Set tenant id and save it to the cookie, then fetch tenant data
   * @param tenantId Tenant ID
   */
  setTenantId(tenantId: string): void {
    this.tenantId = tenantId;
    this.setTenantCookie(tenantId);
    this.fetchTenantData(tenantId);
  }

  /**
   * Get tenant id
   */
  getTenantId(): string | null {
    return this.tenantId;
  }

  /**
   * Append tenant header 'X-Tenant' to the headers
   * @param headers HttpHeaders
   * @returns Updated HttpHeaders
   */
  generateTenantHeaders(headers: HttpHeaders): HttpHeaders {
    if (!this.tenantId) {
      //throw new Error("TenantId is not set");
      return headers;
    }

    return headers.append("X-Tenant", this.tenantId);
  }

  /**
   * Check if the tenant has an active subscription
   * If the tenant is not required to have a subscription, it returns true
   * @returns True if the tenant has an active subscription, otherwise false
   */
  isSubscriptionActive(): boolean {
    if (!this.tenantData) {
      return false;
    }

    // If subscription is null, it means the tenant is not required to have a subscription
    if (this.tenantData.subscription === null) {
      return true;
    }

    return this.tenantData?.subscription?.status === SubscriptionStatus.Active;
  }

  private setTenantCookie(tenantId: string) {
    if (!tenantId) {
      return;
    }

    const currentTenant = this.cookieService.getCookie("X-Tenant");

    if (tenantId === currentTenant) {
      return;
    }

    this.cookieService.setCookie({
      name: "X-Tenant",
      value: tenantId,
      session: true,
    });
  }

  /**
   * Fetch tenant data and save it to the tenantData
   * @param tenantId The tenant ID
   */
  private fetchTenantData(tenantId: string): void {
    this.apiService.getTenant(tenantId).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      this.tenantData = result.data;
      console.log("Tenant data:", result.data);
    });
  }
}
