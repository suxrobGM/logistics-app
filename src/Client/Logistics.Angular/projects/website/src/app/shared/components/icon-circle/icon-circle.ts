import { NgStyle } from "@angular/common";
import { Component, input } from "@angular/core";

type IconSize = "sm" | "md" | "lg";
type GradientVariant = "blue-cyan" | "blue-purple";

@Component({
  selector: "web-icon-circle",
  templateUrl: "./icon-circle.html",
  imports: [NgStyle],
})
export class IconCircle {
  public readonly icon = input.required<string>();
  public readonly size = input<IconSize>("md");
  public readonly gradient = input<GradientVariant>("blue-cyan");
  public readonly hoverScale = input(false);

  protected containerClasses(): string {
    const size = this.size();
    const gradient = this.gradient();
    const hoverScale = this.hoverScale();

    let classes = "flex items-center justify-center text-white transition-transform duration-300";

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

    // Gradient classes
    classes += gradient === "blue-purple"
      ? " bg-linear-to-br from-blue-500 to-purple-500"
      : " bg-linear-to-br from-blue-500 to-cyan-500";

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
