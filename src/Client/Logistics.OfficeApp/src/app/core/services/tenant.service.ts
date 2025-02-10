import {HttpHeaders} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {TenantDto} from "@/core/models";
import {CookieService} from "./cookie.service";

@Injectable({providedIn: "root"})
export class TenantService {
  private tenantId: string | null = null;
  private tenantData: TenantDto | null = null;

  constructor(private readonly cookieService: CookieService) {}

  getTenantData(): TenantDto | null {
    return this.tenantData;
  }

  setTenantData(value: TenantDto): void {
    if (this.tenantData === value) {
      return;
    }

    this.tenantData = value;
  }

  /**
   * Set tenant id and save it to the cookie
   * @param tenantId Tenant id
   */
  setTenantId(tenantId: string): void {
    this.tenantId = tenantId;
    this.setTenantCookie(tenantId);
    console.log("TenantId set to:", tenantId);
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
}
