import { DatePipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router } from "@angular/router";
import type { UserTenantAccessDto } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { AuthService } from "@/core/auth";
import { TenantContextService } from "@/core/services";

@Component({
  selector: "cp-select-tenant",
  templateUrl: "./select-tenant.html",
  imports: [DatePipe, ButtonModule, CardModule, ProgressSpinnerModule],
})
export class SelectTenant {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantContextService);

  protected readonly tenants = this.tenantService.availableTenants;
  protected readonly isLoading = this.tenantService.isLoading;

  constructor() {
    // Load tenants when page is accessed (needed after clearContext from "Switch Company")
    this.tenantService.loadTenants();
  }

  protected selectTenant(tenant: UserTenantAccessDto): void {
    this.tenantService.selectTenant(tenant);
    this.router.navigate(["/"]);
  }

  protected logout(): void {
    this.authService.logout();
  }
}
