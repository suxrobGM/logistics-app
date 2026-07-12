import { booleanAttribute, Component, computed, input } from "@angular/core";
import { NgIcon } from "@ng-icons/core";
import { ERROR_ICON_SVG, UI_ICONS, type IconName } from "../../icons/icons";

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
 * `name` is an {@link IconName} — a key of `UI_ICONS`. A name outside that union is a compile error
 * rather than a silently blank <svg>. The glyph's SVG is bound directly, so there is nothing to
 * register.
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

  /**
   * The glyph's raw SVG, bound to `<ng-icon [svg]>` — which bypasses the registry entirely. An unknown
   * name is a compile error at every static call site, so `undefined` here is only reachable through a
   * dynamic `[name]` binding: log it and render a VISIBLE error glyph rather than a silently blank box.
   */
  protected readonly svg = computed(() => {
    const svg = UI_ICONS[this.name()];
    if (svg === undefined) {
      console.error(
        `[ui-icon] Unknown icon name "${this.name()}". Add it to projects/shared/src/lib/ui/icons/icons.ts.`,
      );
      return ERROR_ICON_SVG;
    }
    return svg;
  });

  protected readonly classes = computed(() =>
    [sizeClasses[this.size()], colorClasses[this.color()], this.spin() ? "animate-spin" : ""]
      .filter(Boolean)
      .join(" "),
  );
}
