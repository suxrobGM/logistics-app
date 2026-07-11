import { booleanAttribute, Component, computed, input } from "@angular/core";
import { NgIcon } from "@ng-icons/core";
import type { IconName } from "../../icons/icon-registry.generated";
import { resolveNgIcon } from "../../icons/ui-icons";

/**
 * `inherit` emits NO size class at all, so the icon takes its size from the surrounding element.
 * This matters: a consumer rule of the form `[&_ng-icon:not([class*='text-'])]:size-4` (exactly how
 * Helm's button sizes its icons) is defeated by ANY `text-*` class, so an icon inside a button must be
 * able to opt out of emitting one. `md` stays the default — 246 call sites depend on it.
 */
export type IconSize = "inherit" | "xs" | "sm" | "md" | "lg" | "xl";

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
  inherit: "",
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
 * Lucide icon (via `@ng-icons/lucide`) with size, color and spin variants.
 *
 * `name` is an {@link IconName} — every name a call site may write, generated from
 * `tools/codemods/icon-map.json`. A name outside that union is a compile error rather than a silently
 * blank <svg>. The resolved glyph must be registered via `provideIcons(...)` in the portal.
 *
 * The host is `inline-flex`, not the browser default `inline`: `transform` does not apply to a
 * non-replaced inline element, so `animate-spin` on an inline host is a no-op.
 */
@Component({
  selector: "ui-icon",
  templateUrl: "./icon.html",
  imports: [NgIcon],
  host: {
    class: "inline-flex",
  },
})
export class Icon {
  public readonly name = input.required<IconName>();
  public readonly size = input<IconSize>("md");
  public readonly color = input<IconColor>("inherit");

  /** Spins the glyph. primeicons' spin class was a CSS keyframe, not a glyph — this replaces it. */
  public readonly spin = input(false, { transform: booleanAttribute });

  protected readonly iconName = computed(() => resolveNgIcon(this.name()));

  protected readonly classes = computed(() =>
    [sizeClasses[this.size()], colorClasses[this.color()], this.spin() ? "animate-spin" : ""]
      .filter(Boolean)
      .join(" "),
  );
}
