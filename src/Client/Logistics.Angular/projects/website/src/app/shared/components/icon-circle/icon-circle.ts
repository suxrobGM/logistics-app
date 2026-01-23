import { Component, input } from "@angular/core";

type IconSize = "sm" | "md" | "lg";
type ColorVariant = "accent" | "ink" | "outlined";

@Component({
  selector: "web-icon-circle",
  templateUrl: "./icon-circle.html",
})
export class IconCircle {
  public readonly icon = input.required<string>();
  public readonly size = input<IconSize>("md");
  public readonly variant = input<ColorVariant>("accent");
  public readonly hoverScale = input(false);

  protected containerClasses(): string {
    const size = this.size();
    const variant = this.variant();
    const hoverScale = this.hoverScale();

    let classes = "flex items-center justify-center transition-all duration-300";

    // Size classes
    switch (size) {
      case "sm":
        classes += " h-10 w-10 rounded-lg";
        break;
      case "lg":
        classes += " h-24 w-24 rounded-full";
        break;
      default:
        classes += " h-14 w-14 rounded-xl";
    }

    // Color variant classes - solid colors, no gradients
    switch (variant) {
      case "ink":
        classes += " bg-ink text-paper";
        break;
      case "outlined":
        classes += " border-2 border-ink/20 text-ink bg-transparent";
        break;
      default: // accent
        classes += " bg-accent text-paper";
    }

    // Hover scale
    if (hoverScale) {
      classes += " group-hover:scale-110";
    }

    return classes;
  }

  protected iconClasses(): string {
    return `pi ${this.icon()}`;
  }

  protected iconStyles(): Record<string, string> {
    const size = this.size();

    switch (size) {
      case "sm":
        return { "font-size": "1.25rem" }; // 20px
      case "lg":
        return { "font-size": "2.5rem" }; // 40px
      default:
        return { "font-size": "1.5rem" }; // 24px
    }
  }
}
