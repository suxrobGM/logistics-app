import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { AuthService } from "@/core/auth";

interface NavItem {
  label: string;
  icon: string;
  routerLink: string;
}

@Component({
  selector: "adm-sidebar",
  templateUrl: "./sidebar.html",
  imports: [RouterModule, ButtonModule],
})
export class Sidebar {
  private readonly authService = inject(AuthService);

  protected readonly navItems: NavItem[] = [
    { label: "Dashboard", icon: "pi pi-home", routerLink: "/home" },
    { label: "Demo Requests", icon: "pi pi-inbox", routerLink: "/demo-requests" },
    { label: "Contact Submissions", icon: "pi pi-envelope", routerLink: "/contact-submissions" },
    { label: "Tenants", icon: "pi pi-building", routerLink: "/tenants" },
    { label: "Subscription Plans", icon: "pi pi-credit-card", routerLink: "/subscription-plans" },
    { label: "Subscriptions", icon: "pi pi-users", routerLink: "/subscriptions" },
    { label: "Users", icon: "pi pi-user", routerLink: "/users" },
    { label: "Blog Posts", icon: "pi pi-file-edit", routerLink: "/blog-posts" },
  ];

  protected readonly userName = this.authService.userName;

  protected logout(): void {
    this.authService.logout();
  }
}
