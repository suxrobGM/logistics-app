import { Component, computed, input } from "@angular/core";
import { NgIcon } from "@ng-icons/core";
import { resolveNgIcon } from "../../icons/ui-icons";

export type IconSize = "xs" | "sm" | "md" | "lg" | "xl";
export type IconColor =
  | "inherit"
  | "primary"
  | "secondary"
  | "muted"
  | "success"
  | "warning"
  | "danger"
  | "info";

const sizeClasses: Record<IconSize, string> = {
  xs: "text-xs",
  sm: "text-sm",
  md: "text-base",
  lg: "text-lg",
  xl: "text-2xl",
};

const colorClasses: Record<IconColor, string> = {
  inherit: "",
  primary: "text-foreground",
  secondary: "text-subtle-foreground",
  muted: "text-muted-foreground",
  success: "text-[var(--success)]",
  warning: "text-[var(--warning)]",
  danger: "text-[var(--danger)]",
  info: "text-[var(--info)]",
};

/**
 * Lucide icon (via @ng-icons/lucide) with size and color variants. Accepts the legacy
 * PrimeIcons-style `name` (with or without the `pi-` prefix); known names are mapped to a lucide
 * icon through {@link PI_TO_LUCIDE}, and any other name is treated as an already-lucide kebab name.
 * The resolved icon must be registered via `provideIcons(...)` in the portal.
 */
@Component({
  selector: "ui-icon",
  templateUrl: "./icon.html",
  imports: [NgIcon],
})
export class Icon {
  public readonly name = input.required<string>();
  public readonly size = input<IconSize>("md");
  public readonly color = input<IconColor>("inherit");

  protected readonly iconName = computed(() => resolveNgIcon(this.name()));

  protected readonly classes = computed(() =>
    [sizeClasses[this.size()], colorClasses[this.color()]].filter(Boolean).join(" "),
  );
}
