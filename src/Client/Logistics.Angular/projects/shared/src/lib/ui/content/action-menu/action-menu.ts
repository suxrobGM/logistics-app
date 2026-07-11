import { Component, computed, input, viewChild } from "@angular/core";
import type { MenuItem } from "primeng/api";
import { Menu, MenuModule } from "primeng/menu";
import { UiButton } from "../../action/button/button";
import type { IconName } from "../../icons/icon-registry.generated";

export interface ActionMenuItem {
  label: string;
  icon?: string;
  hidden?: boolean;
  disabled?: boolean;
  danger?: boolean;
  action: () => void;
}

export type ActionMenuTrigger = "icon" | "button";

/**
 * Standard `pi-ellipsis-v` row context menu. Pass `items` as an array of
 * `{ label, icon?, action, disabled?, hidden?, danger? }`. Wraps PrimeNG `<p-menu>`.
 */
@Component({
  selector: "ui-action-menu",
  templateUrl: "./action-menu.html",
  imports: [MenuModule, UiButton],
})
export class ActionMenu {
  public readonly items = input.required<ActionMenuItem[]>();
  public readonly appendTo = input<"body" | "self">("body");
  public readonly trigger = input<ActionMenuTrigger>("icon");
  public readonly buttonLabel = input<string>("Actions");

  /**
   * The TRIGGER's glyph, now typed `IconName` because it feeds `<ui-button [icon]>` — an unknown
   * name is a compile error rather than a blank button. `items[].icon` below is a different thing
   * and is still a primeicons class string: it feeds PrimeNG's `MenuItem.icon` on `<p-menu>`, which
   * renders it as a CSS class. That one migrates with the menu in S9.
   */
  public readonly buttonIcon = input<IconName>("ellipsis-v");

  protected readonly menu = viewChild<Menu>("menu");

  protected readonly menuItems = computed<MenuItem[]>(() =>
    this.items()
      .filter((item) => !item.hidden)
      .map((item) => ({
        label: item.label,
        icon: item.icon ? `pi pi-${item.icon.replace(/^pi-?/, "")}` : undefined,
        disabled: item.disabled,
        styleClass: item.danger ? "text-[var(--danger)]" : undefined,
        command: () => item.action(),
      })),
  );

  protected readonly appendToTarget = computed(() => (this.appendTo() === "body" ? "body" : null));

  protected toggle(event: Event): void {
    this.menu()?.toggle(event);
  }
}
