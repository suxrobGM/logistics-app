import {Component, signal} from "@angular/core";
import {CommonModule} from "@angular/common";
import {TooltipModule} from "primeng/tooltip";
import {SplitButtonModule} from "primeng/splitbutton";
import {PanelMenuModule} from "primeng/panelmenu";
import {ButtonModule} from "primeng/button";
import {MenuItem} from "primeng/api";
import {globalConfig} from "@/configs";
import {AuthService} from "@/core/auth";
import {sidebarNavItems} from "@/components/layout/data";
import {PanelMenuComponent} from "../panel-menu";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrl: "./sidebar.component.scss",
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
  public readonly navItems = sidebarNavItems;
  public readonly profileMenuItems: MenuItem[];

  constructor(private readonly authService: AuthService) {
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
  }

  toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  logout(): void {
    this.authService.logout();
  }

  openAccountUrl(): void {
    window.open(`${globalConfig.idHost}/account/manage/profile`, "_blank");
  }
}
