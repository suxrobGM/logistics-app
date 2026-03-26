import { Component, computed, inject } from "@angular/core";
import { Router } from "@angular/router";
import { Permission } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { DrawerModule } from "primeng/drawer";
import { AuthService } from "@/core/auth";
import { LayoutService } from "@/core/services/layout.service";

interface MobileNavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

@Component({
  selector: "adm-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  styleUrl: "./mobile-drawer.css",
  imports: [DrawerModule, ButtonModule],
})
export class MobileDrawer {
  private readonly router = inject(Router);
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);
  private readonly permissionService = inject(PermissionService);

  protected readonly visible = this.layoutService.mobileMenuOpen;
  protected readonly userName = this.authService.userName;

  private readonly allNavItems: MobileNavItem[] = [
    { label: "Dashboard", icon: "pi pi-home", route: "/home" },
    { label: "Demo Requests", icon: "pi pi-inbox", route: "/demo-requests" },
    { label: "Contact Submissions", icon: "pi pi-envelope", route: "/contact-submissions" },
    {
      label: "Tenants",
      icon: "pi pi-building",
      route: "/tenants",
      permission: Permission.Tenant.View,
    },
    {
      label: "Features",
      icon: "pi pi-th-large",
      route: "/features",
      permission: Permission.Tenant.Manage,
    },
    { label: "Subscription Plans", icon: "pi pi-credit-card", route: "/subscription-plans" },
    {
      label: "Subscriptions",
      icon: "pi pi-users",
      route: "/subscriptions",
      permission: Permission.Tenant.View,
    },
    { label: "Users", icon: "pi pi-user", route: "/users", permission: Permission.User.Manage },
    {
      label: "Blog Posts",
      icon: "pi pi-file-edit",
      route: "/blog-posts",
      permission: Permission.BlogPost.Manage,
    },
  ];

  protected readonly navItems = computed(() => {
    return this.allNavItems.filter((item) => {
      if (!item.permission) return true;
      return this.permissionService.hasPermission(item.permission);
    });
  });

  protected onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.layoutService.closeMobileMenu();
    }
  }

  protected navigateTo(route: string): void {
    this.router.navigateByUrl(route);
    this.layoutService.closeMobileMenu();
  }

  protected logout(): void {
    this.layoutService.closeMobileMenu();
    this.authService.logout();
  }
}
