import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { ThemeToggle } from "@logistics/shared";
import {
  Icon,
  UiButton,
  UiMenu,
  UiSelectField,
  type IconName,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { TenantContextService } from "@/core/services";
import { environment } from "@/env";

interface NavItem {
  label: string;
  icon: IconName;
  route: string;
}

@Component({
  selector: "cp-navbar",
  templateUrl: "./navbar.html",
  imports: [Icon, RouterLink, RouterLinkActive, ThemeToggle, UiButton, UiMenu, UiSelectField],
})
export class Navbar {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantContextService);

  protected readonly userName = signal<string | null>(null);
  protected readonly currentTenant = this.tenantService.currentTenant;
  protected readonly availableTenants = this.tenantService.availableTenants;
  protected readonly hasMultipleTenants = this.tenantService.hasMultipleTenants;

  protected readonly companyDisplayName = computed(() => {
    const tenant = this.currentTenant();
    return tenant?.companyName ?? tenant?.tenantName ?? "Select Company";
  });

  protected readonly navItems: NavItem[] = [
    { label: "Dashboard", icon: "home", route: "/" },
    { label: "Shipments", icon: "truck", route: "/shipments" },
    { label: "Invoices", icon: "receipt", route: "/invoices" },
    { label: "Documents", icon: "file", route: "/documents" },
  ];

  protected readonly profileMenuItems: UiMenuItem[] = [
    {
      label: "Account Settings",
      icon: "settings",
      routerLink: "/account",
    },
    {
      label: "Manage Profile",
      icon: "user",
      command: () => this.openAccountUrl(),
    },
    {
      label: "Switch Company",
      icon: "building-2",
      command: () => this.switchTenant(),
    },
    { separator: true },
    {
      label: "Sign Out",
      icon: "log-out",
      command: () => this.logout(),
    },
  ];

  constructor() {
    this.authService.onUserDataChanged().subscribe((userData) => {
      if (userData?.getFullName()) {
        this.userName.set(userData.getFullName());
      }
    });
  }

  /**
   * The select is keyed by `tenantId` rather than the DTO object: binding whole objects would make
   * the trigger label depend on reference identity between `currentTenant()` and a member of
   * `availableTenants()`.
   */
  protected onTenantChange(tenantId: string | null): void {
    const tenant = this.availableTenants().find((t) => t.tenantId === tenantId);
    if (!tenant) return;
    this.tenantService.selectTenant(tenant);
    // Reload current page to refresh data with new tenant
    window.location.reload();
  }

  protected logout(): void {
    this.tenantService.clearContext();
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }

  protected switchTenant(): void {
    // Clear current tenant selection and navigate to tenant selection page
    this.tenantService.clearContext();
    this.router.navigate(["/select-tenant"]);
  }
}
