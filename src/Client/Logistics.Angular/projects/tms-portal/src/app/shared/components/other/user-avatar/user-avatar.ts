import { Component, input } from "@angular/core";
import { AvatarModule } from "primeng/avatar";

export type AvatarSize = "normal" | "large" | "xlarge";
export interface AvatarColors {
  bg: string;
  text: string;
}

@Component({
  selector: "app-user-avatar",
  templateUrl: "./user-avatar.html",
  imports: [AvatarModule],
})
export class UserAvatar {
  public readonly initials = input.required<string>();
  public readonly size = input<AvatarSize>("normal");
  public readonly colors = input<AvatarColors>({
    bg: "color-mix(in srgb, var(--primary-500) 15%, transparent)",
    text: "var(--primary-500)",
  });

  protected get avatarStyle() {
    const c = this.colors();
    return {
      "background-color": c.bg,
      color: c.text,
    };
  }
}
