import { Component, input } from "@angular/core";
import { classes } from "../../primitives/utils";

export type UiAvatarSize = "normal" | "large" | "xlarge";

/**
 * The initials avatar: an initials-only circle, never an image or an icon. `shape` is not an input —
 * a non-circular avatar has no caller.
 *
 * The HOST is the circle, so a call site can tint it per user with
 * `[style]="{ 'background-color': …, color: … }"`. The base `bg-*`/`text-*` classes below are only the
 * fallback for sites that pass nothing.
 *
 * @example
 * <ui-avatar [label]="initials()" size="large" />
 */
@Component({
  selector: "ui-avatar",
  templateUrl: "./avatar.html",
})
export class Avatar {
  public readonly label = input.required<string>();

  /** The three sizes: normal 2rem / large 3rem / xlarge 4rem, with the font scaled 1 / 1.5 / 2rem. */
  public readonly size = input<UiAvatarSize>("normal");

  constructor() {
    classes(() => [
      "inline-flex shrink-0 select-none items-center justify-center rounded-full bg-muted text-foreground",
      SIZE_CLASSES[this.size()],
    ]);
  }
}

const SIZE_CLASSES: Record<UiAvatarSize, string> = {
  normal: "size-8 text-base",
  large: "size-12 text-2xl",
  xlarge: "size-16 text-[2rem]",
};
