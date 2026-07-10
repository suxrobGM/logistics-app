import { Component, computed, input, viewChild } from "@angular/core";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { Menu, MenuModule } from "primeng/menu";

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
  imports: [ButtonModule, MenuModule],
})
export class ActionMenu {
  public readonly items = input.required<ActionMenuItem[]>();
  public readonly appendTo = input<"body" | "self">("body");
  public readonly trigger = input<ActionMenuTrigger>("icon");
  public readonly buttonLabel = input<string>("Actions");
  public readonly buttonIcon = input<string>("pi pi-ellipsis-v");

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
