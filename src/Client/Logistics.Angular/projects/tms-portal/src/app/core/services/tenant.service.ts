import { HttpHeaders } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { CookieService } from "@logistics/shared";
import { Api, type TenantDto, getTenantById } from "@logistics/shared/api";

@Injectable({ providedIn: "root" })
export class TenantService {
  private readonly cookieService = inject(CookieService);
  private readonly api = inject(Api);

  private readonly tenantId = signal<string | null>(null);
  private readonly _tenantData = signal<TenantDto | null>(null);

  /**
   * Signal that holds the current tenant data.
   * Use effect() to react to changes.
   */
  public readonly tenantData = this._tenantData.asReadonly();

  getTenantData(): TenantDto | null {
    return this._tenantData();
  }

  /**
   * Set tenant id and save it to the cookie, then fetch tenant data
   * @param tenantId Tenant ID
   */
  setTenantId(tenantId: string): void {
    this.tenantId.set(tenantId);
    this.setTenantCookie(tenantId);
    this.fetchTenantData(tenantId);
  }

  /**
   * Get tenant id
   */
  getTenantId(): string | null {
    return this.tenantId();
  }

  /**
   * Append tenant header 'X-Tenant' to the headers
   * @param headers HttpHeaders
   * @returns Updated HttpHeaders
   */
  generateTenantHeaders(headers: HttpHeaders): HttpHeaders {
    const tenantId = this.tenantId();
    if (!tenantId) {
      return headers;
    }

    return headers.append("X-Tenant", tenantId);
  }

  /**
   * Check if the tenant has an active subscription
   * If the tenant is not required to have a subscription, it returns true
   * @returns True if the tenant has an active subscription, otherwise false
   */
  isSubscriptionActive(): boolean {
    const data = this._tenantData();
    if (!data) {
      return false;
    }

    // If subscription is null, it means the tenant is not required to have a subscription
    if (data.subscription == null) {
      return true;
    }

    return data.subscription.status === "active" || data.subscription.status === "trialing";
  }

  /**
   * Refetch tenant data from the server.
   * Updates the tenantData signal with fresh data.
   */
  public refetchTenantData(): void {
    const tenantId = this.tenantId();
    if (!tenantId) {
      return;
    }

    this.fetchTenantData(tenantId);
  }

  private setTenantCookie(tenantId: string): void {
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
   * Fetch tenant data and save it to the tenantData signal
   * @param tenantId The tenant ID
   */
  private async fetchTenantData(tenantId: string): Promise<void> {
    const tenant = await this.api.invoke(getTenantById, { identifier: tenantId });

    if (!tenant) {
      return;
    }

    this._tenantData.set(tenant);
    console.log("Fetched tenant data:", tenant);
  }
}
