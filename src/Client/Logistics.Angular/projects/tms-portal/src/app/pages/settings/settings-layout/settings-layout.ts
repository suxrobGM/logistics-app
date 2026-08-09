import { Component, computed, inject } from "@angular/core";
import { RouterLink, RouterLinkActive, RouterOutlet } from "@angular/router";
import { FeatureService } from "@logistics/shared/services";
import { PermissionService } from "@/core/auth";
import { passesAccessGate } from "@/shared/layout/nav-menu";
import { systemNav } from "@/shared/layout/sidebar/nav/system.nav";

/**
 * Tabbed shell for the settings pages. The tab bar is a row of plain `routerLink` anchors (styled to
 * mimic `ui-tab-list`) rather than `ui-tabs`, because the shared tabs component isn't router-aware.
 *
 * Tabs come from the `settings` nav item's children so they can't drift from the sidebar. The
 * feature/permission filtering is repeated here rather than reusing `SidebarNavService`, which also
 * applies role filtering we don't want on a page the route guard already admitted.
 */
@Component({
  selector: "app-settings-layout",
  templateUrl: "./settings-layout.html",
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
})
export class SettingsLayoutComponent {
  private readonly featureService = inject(FeatureService);
  private readonly permissionService = inject(PermissionService);

  protected readonly tabs = computed(() =>
    (systemNav.items.find((item) => item.id === "settings")?.children ?? [])
      .filter((child) => passesAccessGate(child, this.featureService, this.permissionService))
      .map((child) => ({ label: child.label, route: child.route! })),
  );
}
