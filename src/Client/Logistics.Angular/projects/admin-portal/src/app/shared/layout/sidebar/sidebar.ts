import { Component, computed, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Permission } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { AuthService } from "@/core/auth";

interface NavItem {
  label: string;
  icon: string;
  routerLink: string;
  permission?: string;
}

@Component({
  selector: "adm-sidebar",
  templateUrl: "./sidebar.html",
  imports: [RouterModule, ButtonModule],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly permissionService = inject(PermissionService);

  private readonly allNavItems: NavItem[] = [
    { label: "Dashboard", icon: "pi pi-home", routerLink: "/home" },
    { label: "Demo Requests", icon: "pi pi-inbox", routerLink: "/demo-requests" },
    { label: "Contact Submissions", icon: "pi pi-envelope", routerLink: "/contact-submissions" },
    {
      label: "Tenants",
      icon: "pi pi-building",
      routerLink: "/tenants",
      permission: Permission.Tenant.View,
    },
    {
      label: "Features",
      icon: "pi pi-th-large",
      routerLink: "/features",
      permission: Permission.Tenant.Manage,
    },
    { label: "Subscription Plans", icon: "pi pi-credit-card", routerLink: "/subscription-plans" },
    {
      label: "Subscriptions",
      icon: "pi pi-users",
      routerLink: "/subscriptions",
      permission: Permission.Tenant.View,
    },
    {
      label: "Users",
      icon: "pi pi-user",
      routerLink: "/users",
      permission: Permission.User.Manage,
    },
    {
      label: "Blog Posts",
      icon: "pi pi-file-edit",
      routerLink: "/blog-posts",
      permission: Permission.BlogPost.Manage,
    },
  ];

  protected readonly navItems = computed(() => {
    return this.allNavItems.filter((item) => {
      if (!item.permission) return true;
      return this.permissionService.hasPermission(item.permission);
    });
  });

  protected readonly userName = this.authService.userName;

  protected logout(): void {
    this.authService.logout();
  }
}
