import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {MenuItem} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {PanelMenuModule} from "primeng/panelmenu";
import {SplitButtonModule} from "primeng/splitbutton";
import {TooltipModule} from "primeng/tooltip";
import {sidebarNavItems} from "@/components/layout/data";
import {AuthService} from "@/core/auth";
import {TenantService} from "@/core/services";
import {environment} from "@/env";
import {PanelMenuComponent} from "../panel-menu";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrl: "./sidebar.component.css",
  imports: [
    CommonModule,
    TooltipModule,
    ButtonModule,
    SplitButtonModule,
    PanelMenuModule,
    PanelMenuComponent,
  ],
})
export class SidebarComponent {
  public readonly isOpened = signal(true);
  public readonly companyName = signal<string | null>(null);
  public readonly userRole = signal<string | null>(null);
  public readonly userFullName = signal<string | null>(null);
  public readonly navItems = signal<MenuItem[]>(sidebarNavItems);
  public readonly profileMenuItems: MenuItem[];

  constructor(
    private readonly authService: AuthService,
    private readonly tenantService: TenantService
  ) {
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
      if (!this.tenantService.isSubscriptionActive()) {
        this.navItems.set(sidebarNavItems.filter((item) => item.label !== "Subscription"));
      }

      this.companyName.set(tenantData?.companyName ?? null);
    });
  }

  toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  logout(): void {
    this.authService.logout();
  }

  openAccountUrl(): void {
    window.open(`${environment.idHost}/account/manage/profile`, "_blank");
  }
}
