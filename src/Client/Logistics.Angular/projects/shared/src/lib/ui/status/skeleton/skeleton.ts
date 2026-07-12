import { Component, computed, input } from "@angular/core";
import { classes } from "../../primitives/utils";

export type UiSkeletonShape = "rect" | "circle";

/**
 * The loading placeholder.
 *
 * Helm's skeleton (four utility classes) is inlined below rather than composed in. The styled box has
 * to BE the host, or call sites' `class=` would restyle a wrapper instead of the grey box — but
 * `hostDirectives: [HlmSkeleton]` fails ng-packagr's NG3001, since a host directive is public API and
 * would force the deliberately-private `HlmSkeleton` to be re-exported from `@logistics/shared/ui`.
 *
 * Sizes are raw CSS lengths (`height="3rem"`, `width="120px"`, `height="100%"`), hence inline styles;
 * as classes each distinct length would need its own arbitrary-value class.
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

  public readonly shape = input<UiSkeletonShape>("rect");

  /**
   * One length for BOTH axes, for `shape="circle"` — a circle whose width and height disagree is an
   * ellipse.
   */
  public readonly size = input<string | null>(null);

  protected readonly resolvedWidth = computed(() => this.size() ?? this.width());
  protected readonly resolvedHeight = computed(() => this.size() ?? this.height());

  constructor() {
    // `classes()` twMerges the call site's `class` LAST, so a site that repaints the background beats
    // `bg-muted` instead of racing it in stylesheet order. `motion-safe:` respects
    // `prefers-reduced-motion`.
    classes(() => [
      "bg-muted block motion-safe:animate-pulse",
      this.shape() === "circle" ? "rounded-full" : "rounded-[var(--ui-radius-content)]",
    ]);
  }
}
