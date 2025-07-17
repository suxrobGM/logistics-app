import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {MenuItem} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {PanelMenuModule} from "primeng/panelmenu";
import {SplitButtonModule} from "primeng/splitbutton";
import {TooltipModule} from "primeng/tooltip";
import {AuthService} from "@/core/auth";
import {TenantService} from "@/core/services";
import {environment} from "@/env";
import {PanelMenu} from "../panel-menu";
import {sidebarItems} from "./sidebar-items";

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
  ],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  public readonly isOpened = signal(true);
  public readonly companyName = signal<string | null>(null);
  public readonly userRole = signal<string | null>(null);
  public readonly userFullName = signal<string | null>(null);
  public readonly navItems = signal<MenuItem[]>(sidebarItems);
  public readonly profileMenuItems: MenuItem[];

  constructor() {
    this.profileMenuItems = [
      {
        label: "User name",
        icon: "bi bi-person-circle h3",
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
      if (userData?.getFullName()) {
        this.userFullName.set(userData.getFullName());
        this.profileMenuItems[0].label = userData.getFullName();
      }

      this.userRole.set(this.authService.getUserRoleName());
    });

    this.tenantService.tenantDataChanged$.subscribe((tenantData) => {
      if (tenantData?.subscription == null || !this.tenantService.isSubscriptionActive()) {
        this.navItems.set(sidebarItems.filter((item) => item.label !== "Subscription"));
      }

      this.companyName.set(tenantData?.companyName ?? null);
    });
  }

  protected toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.idHost}/account/manage/profile`, "_blank");
  }
}
