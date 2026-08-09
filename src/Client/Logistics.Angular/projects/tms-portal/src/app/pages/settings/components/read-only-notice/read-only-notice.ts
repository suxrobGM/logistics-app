import { Component, computed, inject, input } from "@angular/core";
import { Alert } from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";

/**
 * Banner for a settings tab a role can read but not write. Renders nothing when the user holds
 * `permission`, so a tab can drop it in unconditionally.
 */
@Component({
  selector: "app-read-only-notice",
  imports: [Alert],
  template: `
    @if (!canManage()) {
      <ui-alert intent="info">
        You can view these settings but not change them. Ask an owner to update them.
      </ui-alert>
    }
  `,
})
export class ReadOnlyNotice {
  private readonly permissionService = inject(PermissionService);

  /** The permission a user needs to edit this tab. */
  public readonly permission = input.required<string>();

  protected readonly canManage = computed(() =>
    this.permissionService.hasPermission(this.permission()),
  );
}
