import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { PanelMenuModule } from "primeng/panelmenu";
import { SplitButtonModule } from "primeng/splitbutton";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import { TenantService } from "@/core/services";
import { environment } from "@/env";
import { PanelMenu } from "../panel-menu";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { sidebarItems } from "./sidebar-items";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.html",
  styleUrl: "./sidebar.css",
  imports: [
    CommonModule,
    TooltipModule,
    ButtonModule,
    SplitButtonModule,
    PanelMenuModule,
    PanelMenu,
    ThemeToggle,
  ],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);

  public readonly isOpened = signal(true);
  public readonly companyName = signal<string | null>(null);
  public readonly companyLogoUrl = signal<string | null>(null);
  public readonly userRole = signal<string | null>(null);
  public readonly userFullName = signal<string | null>(null);
  public readonly navItems = signal<MenuItem[]>(sidebarItems);
  public readonly profileMenuItems: MenuItem[];

  constructor() {
    this.profileMenuItems = [
      {
        label: "User name",
        icon: "pi pi-user text-3xl!",
        items: [
          {
            label: "Profile",
            command: () => this.openAccountUrl(),
          },
          {
            separator: true,
          },
          {
            label: "Sign out",
            command: () => this.logout(),
          },
        ],
      },
    ];

    this.authService.onUserDataChanged().subscribe((userData) => {
      if (!userData) {
        return; // Wait until user data is available before processing
      }

      if (userData.getFullName()) {
        this.userFullName.set(userData.getFullName());
        this.profileMenuItems[0].label = userData.getFullName();
      }

      const userRole = userData.role;
      this.userRole.set(this.authService.getUserRoleName());

      // Settings menu is only visible to Owner role
      if (userRole !== UserRole.Owner) {
        this.navItems.update((items) => items.filter((item) => item.label !== "Settings"));
      }
    });

    this.tenantService.tenantDataChanged$.subscribe((tenantData) => {
      this.companyName.set(tenantData?.companyName ?? null);
      this.companyLogoUrl.set(tenantData?.logoUrl ?? null);
    });
  }

  protected toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }
}
