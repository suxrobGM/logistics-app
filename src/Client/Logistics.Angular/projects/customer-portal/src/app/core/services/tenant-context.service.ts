import { Injectable, computed, inject, signal } from "@angular/core";
import { Api, CookieService, type UserTenantAccessDto, getPortalTenants } from "@logistics/shared";
import { firstValueFrom } from "rxjs";
import { AuthService } from "@/core/auth";

@Injectable({ providedIn: "root" })
export class TenantContextService {
  private readonly api = inject(Api);
  private readonly cookieService = inject(CookieService);
  private readonly authService = inject(AuthService);

  private readonly _currentTenant = signal<UserTenantAccessDto | null>(null);
  private readonly _availableTenants = signal<UserTenantAccessDto[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _tenantsLoaded = signal(false);

  public readonly currentTenant = this._currentTenant.asReadonly();
  public readonly availableTenants = this._availableTenants.asReadonly();
  public readonly isLoading = this._isLoading.asReadonly();
  public readonly tenantsLoaded = this._tenantsLoaded.asReadonly();

  public readonly hasSingleTenant = computed(() => this._availableTenants().length === 1);
  public readonly hasMultipleTenants = computed(() => this._availableTenants().length > 1);
  public readonly needsTenantSelection = computed(
    () => this.hasMultipleTenants() && !this._currentTenant(),
  );

  /**
   * Load available tenants for the current user from the API
   */
  async loadTenants(): Promise<void> {
    if (this._tenantsLoaded()) {
      return;
    }

    this._isLoading.set(true);

    try {
      // Wait for access token to be available before making API call
      // This prevents 401 errors on first login when token isn't ready yet
      await firstValueFrom(this.authService.getAccessToken());

      const tenants = await this.api.invoke(getPortalTenants);

      this._availableTenants.set(tenants);
      this._tenantsLoaded.set(true);

      // Auto-select if only one tenant
      if (tenants.length === 1) {
        this.selectTenant(tenants[0]);
      } else {
        // Try to restore from cookie
        const savedTenantId = this.cookieService.getCookie("X-Tenant");
        if (savedTenantId) {
          const savedTenant = tenants.find((t) => t.tenantId === savedTenantId);
          if (savedTenant) {
            this.selectTenant(savedTenant);
          }
        }
      }
    } catch (error) {
      console.error("Failed to load tenants:", error);
      this._availableTenants.set([]);
    } finally {
      this._isLoading.set(false);
    }
  }

  /**
   * Select a tenant and store it in cookie
   */
  selectTenant(tenant: UserTenantAccessDto): void {
    this._currentTenant.set(tenant);
    this.cookieService.setCookie({
      name: "X-Tenant",
      value: tenant.tenantId!,
      session: true,
    });
  }

  /**
   * Get the current tenant ID for use in HTTP headers
   */
  getTenantId(): string | null {
    return this._currentTenant()?.tenantId ?? null;
  }

  /**
   * Clear tenant context on logout
   */
  clearContext(): void {
    this._currentTenant.set(null);
    this._availableTenants.set([]);
    this._tenantsLoaded.set(false);
    this.cookieService.deleteCookie("X-Tenant");
  }
}
