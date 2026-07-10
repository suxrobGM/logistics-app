import { Component, computed, input } from "@angular/core";
import { LucideDynamicIcon } from "@lucide/angular";
import { PI_TO_LUCIDE, UI_ICON_FALLBACK } from "../../icons/ui-icons";

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
 * Lucide icon (via @lucide/angular) with size and color variants. Accepts the legacy
 * PrimeIcons-style `name` (with or without the `pi-` prefix); it is mapped to a lucide
 * icon through {@link PI_TO_LUCIDE}, falling back to a circle for unmapped names.
 */
@Component({
  selector: "ui-icon",
  templateUrl: "./icon.html",
  imports: [LucideDynamicIcon],
})
export class Icon {
  public readonly name = input.required<string>();
  public readonly size = input<IconSize>("md");
  public readonly color = input<IconColor>("inherit");

  protected readonly iconName = computed(() => {
    const trimmed = this.name().replace(/^pi-?/, "");
    return PI_TO_LUCIDE[trimmed] ?? UI_ICON_FALLBACK;
  });

  protected readonly classes = computed(() =>
    [sizeClasses[this.size()], colorClasses[this.color()]].filter(Boolean).join(" "),
  );
}
