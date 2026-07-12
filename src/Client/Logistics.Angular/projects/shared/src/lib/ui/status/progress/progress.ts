import { booleanAttribute, Component, computed, input } from "@angular/core";
import { HlmProgress, HlmProgressIndicator } from "../../primitives/progress";
import { classes } from "../../primitives/utils";

/**
 * The determinate progress bar, on Helm/Brain — `hlmProgress` host-directives `BrnProgress`, which is
 * what puts `role="progressbar"` and `aria-valuenow` / `-valuemin` / `-valuemax` on the track, and
 * `hlmProgressIndicator` flips the fill for RTL.
 *
 * The track is an inner element rather than the host because `hostDirectives: [HlmProgress]` fails
 * ng-packagr's NG3001 — a host directive is public API, so it would force the private `HlmProgress`
 * to be re-exported from `@logistics/shared/ui`. Template composition is the supported way.
 *
 * The default height is a CLASS (`h-5`), not an inline style, so that a call site's `class="h-1"` can
 * win it on twMerge; an inline `style.height` default would beat it outright. The `height` input does
 * emit an inline style, which correctly beats both.
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

  /** Renders the percentage, centred in the bar. */
  public readonly showValue = input(false, { transform: booleanAttribute });

  /** Any CSS colour, painted on the FILL. Null keeps Helm's themed `bg-primary`. */
  public readonly color = input<string | null>(null);

  /** Any CSS length. Null (the default) leaves the `h-5` class in charge — see the note above. */
  public readonly height = input<string | null>(null);

  protected readonly label = computed(() => `${Math.round(this.value() ?? 0)}%`);

  constructor() {
    classes(() => "block h-5 w-full rounded-[var(--ui-radius-content)]");
  }
}
