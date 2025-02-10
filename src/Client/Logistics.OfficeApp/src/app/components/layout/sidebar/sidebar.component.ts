import {Component, OnInit, signal} from "@angular/core";
import {CommonModule} from "@angular/common";
import {RouterLink} from "@angular/router";
import {TooltipModule} from "primeng/tooltip";
import {SplitButtonModule} from "primeng/splitbutton";
import {PanelMenuModule} from "primeng/panelmenu";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ButtonModule} from "primeng/button";
import {MenuItem} from "primeng/api";
import {globalConfig} from "@/configs";
import {AuthService} from "@/core/auth";
import {ApiService, TenantService} from "@/core/services";

@Component({
  selector: "app-sidebar",
  standalone: true,
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.scss"],
  imports: [
    CommonModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    SplitButtonModule,
    PanelMenuModule,
    OverlayPanelModule,
  ],
})
export class SidebarComponent implements OnInit {
  public readonly isOpened = signal(false);
  public readonly companyName = signal<string | null>(null);
  public readonly userRole = signal<string | null>(null);
  public readonly userFullName = signal<string | null>(null);
  public readonly profileMenuItems: MenuItem[];
  public readonly accountingMenuItems: MenuItem[];

  constructor(
    private readonly authService: AuthService,
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService
  ) {
    this.profileMenuItems = [
      {
        label: "User name",
        icon: "bi bi-person-circle",
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

    this.accountingMenuItems = [
      {
        label: "Accounting",
        icon: "bi bi-journal-text h1",
        items: [
          {
            label: "Payrolls",
            routerLink: "/accounting/payrolls",
          },
          {
            label: "Payments",
            routerLink: "/accounting/payments",
          },
          {
            label: "Invoices",
            routerLink: "/accounting/invoices",
          },
        ],
      },
    ];
  }

  ngOnInit(): void {
    this.authService.onUserDataChanged().subscribe((userData) => {
      if (userData?.getFullName()) {
        this.userFullName.set(userData.getFullName());
        this.profileMenuItems[0].label = userData.getFullName();
      }

      if (userData?.tenant) {
        this.fetchTenantData(userData?.tenant);
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
  private fetchTenantData(tenantId: string): void {
    this.apiService.getTenant(tenantId).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      this.tenantService.setTenantData(result.data);
      this.companyName.set(result.data.companyName);
    });
  }
}
