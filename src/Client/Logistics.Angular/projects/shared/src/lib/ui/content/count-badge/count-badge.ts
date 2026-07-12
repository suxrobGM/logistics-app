import { Component, computed, input } from "@angular/core";
import { classes } from "../../primitives/utils";
import type { UiBadgeTone } from "../badge/badge-intent";

export type UiCountBadgeSize = "small" | "normal";

/**
 * The count pill. Replaces `<p-badge>` (8 sites) and backs `<ui-overlay-badge>`.
 *
 * =================================================================================================
 * WHY THIS IS NOT `ui-badge`. THEY ARE STRUCTURALLY DIFFERENT COMPONENTS AND THEY LOOK DIFFERENT.
 * =================================================================================================
 * `<p-badge>` and `<p-tag>` share a severity vocabulary and nothing else. They are two token blocks:
 *
 *                       p-badge (this)                     p-tag (ui-badge)
 *     height            1.5rem, FIXED                      auto — grows with its text
 *     min-width         1.5rem  (so "3" is a circle)       none
 *     border-radius     {border.radius.md} = 6px           {content.border.radius} — 2px in Nora
 *     font-weight       700                                500 in TMS (preset override)
 *     colour (Aura)     SOLID {green.500} on {surface.0}   a 16% TINT of the hue
 *
 * Folding the 8 `<p-badge>`s into `ui-badge` would have swapped eight solid count pills — the nav
 * menu's unread counts, the attention panel's alert counts — for soft tinted tags of a different
 * height. All eight live in TMS, where Nora happens to paint tag and badge the same COLOUR, which is
 * exactly the kind of coincidence that makes a bad merge look fine in one portal and wrong in the
 * other three the moment a badge appears there.
 *
 * The fixed height and min-width are the whole point of the component: they are what make a
 * single-digit count render as a circle and a double-digit count as a pill, instead of as two
 * differently-shaped boxes.
 *
 * The tone table is deliberately NOT `ui-badge`'s: p-badge is solid in BOTH presets (Aura and Nora
 * agree here — `{green.500}`/`{green.400}` on `{surface.0}`), so there is no per-app fork to carry
 * and no `--ui-badge-*` indirection. Named utilities, straight through.
 */
@Component({
  selector: "ui-count-badge",
  templateUrl: "./count-badge.html",
})
export class CountBadge {
  public readonly value = input<string | number | null>(null);
  public readonly severity = input<UiBadgeTone>("danger");
  public readonly size = input<UiCountBadgeSize>("normal");

  protected readonly text = computed(() => {
    const value = this.value();
    return value === null || value === "" ? null : String(value);
  });

  constructor() {
    classes(() => [
      "inline-flex shrink-0 items-center justify-center rounded-md font-bold leading-none",
      this.size() === "small" ? "h-5 min-w-5 px-1.5 text-[0.625rem]" : "h-6 min-w-6 px-2 text-xs",
      TONE_CLASSES[this.severity()],
    ]);
  }
}

/**
 * Solid in both presets. `--inverse` is the theme's opposing ink (near-white in light, near-ink in
 * dark) — the same token, and the same reasoning, as `--ui-btn-on-solid`: a saturated fill needs the
 * ink to flip with the theme, and `{surface.0}` / `{green.950}` is exactly that flip.
 */
const TONE_CLASSES: Record<UiBadgeTone, string> = {
  primary: "bg-primary text-primary-foreground",
  secondary: "bg-secondary text-subtle-foreground",
  success: "bg-success text-[var(--inverse)]",
  info: "bg-info text-[var(--inverse)]",
  warn: "bg-warning text-[var(--inverse)]",
  danger: "bg-danger text-[var(--inverse)]",
  contrast: "bg-foreground text-background",
};
