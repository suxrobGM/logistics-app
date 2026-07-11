import { Component, computed, input } from "@angular/core";
import { classes } from "../../primitives/utils";

export type UiSkeletonShape = "rect" | "circle";

/**
 * The loading placeholder. Replaces `<p-skeleton>` at 98 call sites / 31 files.
 *
 * =================================================================================================
 * WHY HELM'S SKELETON IS INLINED HERE RATHER THAN COMPOSED IN (the primitive is deleted)
 * =================================================================================================
 * The natural build is `hostDirectives: [HlmSkeleton]` — the host IS the grey box, so the directive
 * belongs on the host. ng-packagr rejects it:
 *
 *     NG3001: Unsupported private class HlmSkeleton. This class is visible to consumers via
 *             Skeleton -> HlmSkeleton, but is not exported from the top-level library entrypoint.
 *
 * A host directive is part of a component's PUBLIC API (its inputs land on the host), so composing
 * one in would force `HlmSkeleton` to be re-exported from `@logistics/shared/ui` — and the Helm
 * primitives are deliberately private, precisely so feature code cannot reach past `ui-*`. Template
 * composition is the usual escape hatch, but it does not apply here: the styled box has to be the
 * host, or the 15 `class=` and 14 `styleClass=` call sites (including four that repaint the
 * background with `bg-purple-500/20`) would land on a wrapper instead of on the thing they are
 * trying to restyle.
 *
 * Helm's entire skeleton is four utility classes. They are reproduced verbatim below rather than
 * making a private primitive public to import them.
 * =================================================================================================
 *
 * THE ANIMATION CHANGES, and it is the one thing here that is not a reproduction. PrimeNG's skeleton
 * shimmers — a translucent gradient swept across the box by a `::after` pseudo-element on a 1.2s
 * loop. Helm's pulses. Both read as "loading", neither is load-bearing, and `motion-safe:` (the pulse
 * respects `prefers-reduced-motion`; the shimmer did not) is a straight accessibility win.
 *
 * SIZES ARE CSS LENGTHS, NOT TAILWIND: `height="3rem"`, `width="120px"`, `height="100%"`. The call
 * sites pass raw CSS, so these are inline styles. (As classes they would need an arbitrary-value
 * class per distinct length, and there are 20 distinct heights.)
 */
@Component({
  selector: "ui-skeleton",
  templateUrl: "./skeleton.html",
  host: {
    "[style.width]": "resolvedWidth()",
    "[style.height]": "resolvedHeight()",
  },
})
export class Skeleton {
  public readonly width = input<string | null>(null);
  public readonly height = input<string | null>(null);

  /** p-skeleton's `shape`. Only `circle` is ever passed (6 sites); `rect` is the default. */
  public readonly shape = input<UiSkeletonShape>("rect");

  /**
   * p-skeleton's `size` (6 sites): one length for BOTH axes. Only ever used with `shape="circle"`,
   * which is the point of it — a circle whose width and height disagree is an ellipse.
   */
  public readonly size = input<string | null>(null);

  protected readonly resolvedWidth = computed(() => this.size() ?? this.width());
  protected readonly resolvedHeight = computed(() => this.size() ?? this.height());

  constructor() {
    // `classes()` twMerges the call site's `class` LAST, which is what makes the four
    // `bg-purple-500/20` sites actually beat `bg-muted` instead of racing it in stylesheet order.
    // `--ui-radius-content` is the per-app fork (Aura 6px, Nora/TMS 2px) — see theme.css.
    classes(() => [
      "bg-muted block motion-safe:animate-pulse",
      this.shape() === "circle" ? "rounded-full" : "rounded-[var(--ui-radius-content)]",
    ]);
  }
}
