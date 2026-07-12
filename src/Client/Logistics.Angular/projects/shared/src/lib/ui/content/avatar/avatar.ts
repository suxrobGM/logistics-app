import { Component, input } from "@angular/core";
import { classes } from "../../primitives/utils";

export type UiAvatarSize = "normal" | "large" | "xlarge";

/**
 * The initials avatar. Replaces `<p-avatar>` at all 4 call sites.
 *
 * ALL FOUR ARE INITIALS-ONLY CIRCLES — `[label]` + `shape="circle"`, never an image, never an icon.
 * So this component does one thing, and `shape` is not an input: a non-circular avatar has no caller.
 *
 * WHY NOT `hlm-avatar` (we generated it, then deleted it)
 * HlmAvatar is `BrnAvatar` + image-with-fallback machinery: it exists to swap a broken `<img>` for a
 * fallback, and projects both through `<ng-content select>`. With no images anywhere, all of that is
 * dead weight around a `<div>` with two letters in it. Its sizes also disagree with this component's
 * (`sm/default/lg` = 1.5/2/2.5rem, against normal/large/xlarge = 2/3/4rem) and it paints an
 * `after:border` ring this design has no use for. Nothing survived the port.
 *
 * COLOURS COME FROM THE CALL SITE, VIA `[style]`, AND THAT IS ALREADY HOW IT WORKS.
 * `app-user-avatar` tints its circle per user by binding `[style]="{ 'background-color': …, color: … }"`.
 * Because the HOST is the circle here (as `.p-avatar` was p-avatar's host), that binding keeps
 * working untouched — Angular applies `[style]` to the host, which is the painted box. The base
 * `bg-*`/`text-*` classes below are only the fallback for the sites that pass nothing.
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
    // `bg-muted` is Nora's `{content.hover.background}` / Aura's `{content.border.color}` — the two
    // presets differ by one step on the surface ramp and neither is worth a token fork for a default
    // that only shows when a caller passes no colour at all (1 of the 4 sites).
    classes(() => [
      "inline-flex shrink-0 select-none items-center justify-center rounded-full bg-muted text-foreground",
      SIZE_CLASSES[this.size()],
    ]);
  }
}

/** p-avatar's `root` / `lg` / `xl` size tokens, which Nora and Aura agree on exactly. */
const SIZE_CLASSES: Record<UiAvatarSize, string> = {
  normal: "size-8 text-base",
  large: "size-12 text-2xl",
  xlarge: "size-16 text-[2rem]",
};
