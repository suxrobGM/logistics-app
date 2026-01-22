import { Component, computed, input } from "@angular/core";

type AvatarSize = "sm" | "md" | "lg";

@Component({
  selector: "web-avatar",
  templateUrl: "./avatar.html",
})
export class Avatar {
  public readonly initials = input.required<string>();
  public readonly size = input<AvatarSize>("md");
  public readonly hoverScale = input(false);

  protected readonly sizeClasses = computed(() => {
    const sizes: Record<AvatarSize, string> = {
      sm: "h-8 w-8 text-xs",
      md: "h-10 w-10 text-sm",
      lg: "h-24 w-24 text-2xl",
    };
    return sizes[this.size()];
  });
}
