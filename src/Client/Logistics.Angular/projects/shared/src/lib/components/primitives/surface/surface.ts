import { booleanAttribute, Component, computed, input } from "@angular/core";

export type SurfaceVariant = "elevated" | "subtle" | "plain";
export type SurfacePadding = "none" | "sm" | "md" | "lg";
export type SurfaceRadius = "sm" | "md" | "lg";

const variantClasses: Record<SurfaceVariant, string> = {
  elevated: "bg-elevated",
  subtle: "bg-subtle",
  plain: "",
};

const paddingClasses: Record<SurfacePadding, string> = {
  none: "",
  sm: "p-2",
  md: "p-4",
  lg: "p-6",
};

const radiusClasses: Record<SurfaceRadius, string> = {
  sm: "rounded-sm",
  md: "rounded-md",
  lg: "rounded-lg",
};

/**
 * Themed card/section background. `variant` picks `bg-elevated` / `bg-subtle` / no fill;
 * padding, radius, and border are configurable. Always theme-aware (no hardcoded colors).
 */
@Component({
  selector: "ui-surface",
  templateUrl: "./surface.html",
})
export class Surface {
  public readonly variant = input<SurfaceVariant>("elevated");
  public readonly padding = input<SurfacePadding>("md");
  public readonly radius = input<SurfaceRadius>("lg");
  public readonly border = input<boolean, unknown>(true, { transform: booleanAttribute });

  protected readonly classes = computed(() => {
    const parts = [
      variantClasses[this.variant()],
      paddingClasses[this.padding()],
      radiusClasses[this.radius()],
    ];
    if (this.border()) parts.push("border border-default");
    return parts.filter(Boolean).join(" ");
  });
}
