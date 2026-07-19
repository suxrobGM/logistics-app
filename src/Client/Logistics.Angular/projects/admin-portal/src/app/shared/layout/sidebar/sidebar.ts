import { Component, computed, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Converters, ThemeToggle } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { Avatar, Divider, Icon, UiButton } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { sidebarSections, type AdminNavSection } from "./sidebar-items";

@Component({
  selector: "adm-sidebar",
  templateUrl: "./sidebar.html",
  imports: [Avatar, Divider, Icon, RouterModule, ThemeToggle, UiButton],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly permissionService = inject(PermissionService);

  protected readonly sections = computed<AdminNavSection[]>(() => {
    return sidebarSections
      .map((section) => ({
        ...section,
        items: section.items.filter((item) => {
          if (!item.permission) return true;
          return this.permissionService.hasPermission(item.permission);
        }),
      }))
      .filter((section) => section.items.length > 0);
  });

  protected readonly userName = computed(() => this.authService.userName() ?? "Admin");
  protected readonly userInitials = computed(() => Converters.getInitials(this.userName()));

  protected logout(): void {
    this.authService.logout();
  }
}
