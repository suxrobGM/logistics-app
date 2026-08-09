import { booleanAttribute, Component, computed, inject, input } from "@angular/core";
import { openIdentityAccountPage } from "@logistics/shared/auth";
import { Icon, UiMenu, type UiMenuItem } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { environment } from "@/env";

/**
 * The account trigger and its menu, shared by the sidebar rail and the mobile drawer so the two
 * can't drift apart again. Renders nothing until there is a signed-in user.
 */
@Component({
  selector: "app-user-menu",
  templateUrl: "./user-menu.html",
  styleUrl: "./user-menu.css",
  host: {
    class: "block",
    "[class.compact]": "compact()",
    "[class.labels-hidden]": "labelsHidden()",
  },
  imports: [Icon, UiMenu],
})
export class UserMenu {
  private readonly authService = inject(AuthService);

  /** The rail packs this tighter than the drawer does. */
  public readonly compact = input(false, { transform: booleanAttribute });
  /** Collapsed rail: avatar only, no name or role. */
  public readonly labelsHidden = input(false, { transform: booleanAttribute });

  protected readonly userFullName = computed(
    () => this.authService.getUserData()?.getFullName() ?? null,
  );
  protected readonly userRole = computed(() => this.authService.getUserRoleName());

  protected readonly menuItems: UiMenuItem[] = [
    { label: "Manage Profile", icon: "user-pen", command: () => this.openAccountUrl() },
    { label: "Privacy & Data", icon: "shield", command: () => this.openPrivacyUrl() },
    { label: "Sign Out", icon: "log-out", variant: "destructive", command: () => this.logout() },
  ];

  protected logout(): void {
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    openIdentityAccountPage(environment.identityServerUrl, "profile");
  }

  protected openPrivacyUrl(): void {
    openIdentityAccountPage(environment.identityServerUrl, "privacy");
  }
}
