import { booleanAttribute, Component, computed, input } from "@angular/core";
import { HlmProgress, HlmProgressIndicator } from "../../primitives/progress";
import { classes } from "../../primitives/utils";

/**
 * The determinate progress bar. Replaces `<p-progressBar>` at 5 call sites.
 *
 * BACKED BY HELM / BRAIN, and here that buys something real rather than being a box to tick.
 * `hlmProgress` host-directives `BrnProgress`, which is what puts `role="progressbar"` and the
 * `aria-valuenow` / `aria-valuemin` / `aria-valuemax` triple on the track. `<p-progressBar>` had them
 * too; a hand-rolled `<div>` with a coloured child would have had none, and "the AI-quota bar is
 * invisible to a screen reader" is not something a build or a snapshot test catches.
 * `hlmProgressIndicator` also flips the fill for RTL, which p-progressBar did via `inset-inline-*`.
 *
 * WHY THE TRACK IS AN INNER ELEMENT AND NOT THE HOST
 * The obvious build is `hostDirectives: [HlmProgress]`. ng-packagr rejects it — a host directive is
 * part of a component's public API, so it would force the private `HlmProgress` to be re-exported
 * from `@logistics/shared/ui` (NG3001; same wall `ui-skeleton` hit). Putting `hlmProgress` on an
 * inner `<div>` in the TEMPLATE is the supported composition and costs nothing here, because unlike
 * the skeleton this component has no call site that restyles the bar's own box.
 *
 * HEIGHT IS A CLASS, NOT AN INLINE STYLE, UNLESS ASKED FOR. `h-5` (1.25rem) is p-progressBar's
 * default in both presets. It is a class so that the 3 call sites passing `class="h-2!"` / `class="h-1"`
 * win it on twMerge; an inline `style.height` default would have beaten the un-`!`-ed `h-1` outright
 * and silently pinned that bar at 1.25rem. `height` (for the 2 sites that passed `[style]`) still
 * emits an inline style, which correctly beats both.
 *
 * @example
 * <ui-progress [value]="pct()" height="0.5rem" />
 * <ui-progress [value]="pct()" showValue [color]="barColor()" />
 */
@Component({
  selector: "ui-progress",
  templateUrl: "./progress.html",
  imports: [HlmProgress, HlmProgressIndicator],
  host: {
    "[style.height]": "height()",
  },
})
export class Progress {
  public readonly value = input<number | null>(null);

  /** p-progressBar's `[showValue]` — the percentage, centred in the bar. 1 site sets it. */
  public readonly showValue = input(false, { transform: booleanAttribute });

  /**
   * p-progressBar's `[color]` (1 site) — any CSS colour, painted on the FILL. Null keeps Helm's
   * themed `bg-primary`, which is what `progressbar.value.background` (`{primary.color}`) resolved
   * to in both presets anyway.
   */
  public readonly color = input<string | null>(null);

  /** Any CSS length. Null (the default) leaves the `h-5` class in charge — see the note above. */
  public readonly height = input<string | null>(null);

  protected readonly label = computed(() => `${Math.round(this.value() ?? 0)}%`);

  constructor() {
    // `progressbar.borderRadius` is `{content.border.radius}` in both presets — the same shared knob
    // the tag and the skeleton read, which is why there is one variable here and not three.
    classes(() => "block h-5 w-full rounded-[var(--ui-radius-content)]");
  }
}
