import { Component, computed, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Converters, ThemeToggle } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { resolveNgIcon } from "@logistics/shared/ui";
import { NgIcon } from "@ng-icons/core";
import { AvatarModule } from "primeng/avatar";
import { ButtonModule } from "primeng/button";
import { DividerModule } from "primeng/divider";
import { AuthService } from "@/core/auth";
import { sidebarSections, type AdminNavSection } from "./sidebar-items";

@Component({
  selector: "adm-sidebar",
  templateUrl: "./sidebar.html",
  imports: [RouterModule, ButtonModule, AvatarModule, DividerModule, NgIcon, ThemeToggle],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly permissionService = inject(PermissionService);

  /** Maps a nav item's icon name to its registered `@ng-icons/lucide` name. */
  protected readonly ngIcon = resolveNgIcon;

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

  protected readonly userName = this.authService.userName;
  protected readonly userInitials = computed(() => Converters.getInitials(this.userName()));

  protected logout(): void {
    this.authService.logout();
  }
}
