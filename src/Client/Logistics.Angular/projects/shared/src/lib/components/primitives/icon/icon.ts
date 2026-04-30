import { Component, computed, input } from "@angular/core";

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
  primary: "text-primary",
  secondary: "text-secondary",
  muted: "text-muted",
  success: "text-[var(--success)]",
  warning: "text-[var(--warning)]",
  danger: "text-[var(--danger)]",
  info: "text-[var(--info)]",
};

/**
 * PrimeIcons wrapper with size and color variants. Pass `name` without the `pi-` prefix.
 */
@Component({
  selector: "ui-icon",
  templateUrl: "./icon.html",
  host: { class: "contents" },
})
export class Icon {
  public readonly name = input.required<string>();
  public readonly size = input<IconSize>("md");
  public readonly color = input<IconColor>("inherit");

  protected readonly classes = computed(() => {
    const trimmed = this.name().replace(/^pi-?/, "");
    return ["pi", `pi-${trimmed}`, sizeClasses[this.size()], colorClasses[this.color()]]
      .filter(Boolean)
      .join(" ");
  });
}
