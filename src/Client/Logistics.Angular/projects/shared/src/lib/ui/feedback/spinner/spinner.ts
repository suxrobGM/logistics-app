import { Component, input } from "@angular/core";
import { Icon } from "../../content/icon/icon";

/**
 * The busy spinner. Replaces `<p-progress-spinner>` / `<p-progressSpinner>` (63 sites, 63 files —
 * PrimeNG ships both spellings and the repo uses both).
 *
 * =================================================================================================
 * THE DEFAULT IS 100px, AND THAT IS NOT A TYPO.
 * =================================================================================================
 * 50 of the 63 call sites pass NOTHING, so whatever this component defaults to IS the spinner in
 * fifty places. PrimeNG's default is not small:
 *
 *     .p-progressspinner { width: 100px; height: 100px; }      — @primeuix/styles/progressspinner
 *
 * Keeping 100px keeps the BOX, which is the part that would reflow fifty loading states if it
 * changed. Shrinking it to something more tasteful is a design decision, and it belongs in a commit
 * that is about the design and not about the migration — `size` is here so that commit is one-line
 * per site.
 *
 * WHAT DOES CHANGE: THE GLYPH, DELIBERATELY.
 * PrimeNG's spinner is an SVG arc that cycles through four hard-coded hues on a 6s loop —
 * `colorOne: {red.500}, colorTwo: {blue.500}, colorThree: {green.500}, colorFour: {yellow.500}` —
 * the one thing in the whole preset that ignores the palette entirely. It is a Material relic. This
 * renders the same spinning circle every other busy state in the app already uses (`ui-button`'s
 * loading glyph is this exact icon), in `currentColor`, so it inherits the surrounding text colour
 * and themes for free. Same footprint, one colour, no cycling.
 *
 * WHY NOT `hlm-spinner` (we generated it, then deleted it)
 * It is a `<ng-icon>` on `lucideLoader2` with `animate-spin`, registered through its own local
 * `provideIcons`. We already have that glyph — `spinner` → `loader-circle` in the icon registry —
 * and `ui-button` already spins it. Adopting HlmSpinner would mean a second loader icon and a second
 * registration path for the same picture. One icon pipeline.
 *
 * @example
 * <ui-spinner />                                   <!-- 100px, like p-progress-spinner -->
 * <ui-spinner size="24px" ariaLabel="Loading loads" />
 */
@Component({
  selector: "ui-spinner",
  templateUrl: "./spinner.html",
  imports: [Icon],
  host: {
    // `role="status"` + a name is what makes a spinner announce itself; p-progress-spinner had the
    // role built in and 2 sites bothered with `ariaLabel`. Now every site gets a named one.
    role: "status",
    "[attr.aria-label]": "ariaLabel()",
    // inline-flex, not inline: `transform` does not apply to a non-replaced inline box, so
    // `animate-spin` on an inline host is a silent no-op. Same reason ui-icon's host is inline-flex.
    class: "inline-flex items-center justify-center",
    // The glyph is `size="inherit"` — it emits no text-* class and takes the host's font-size. So
    // sizing the host's FONT sizes the spinner, and the host's box matches it exactly.
    "[style.font-size]": "size()",
    "[style.width]": "size()",
    "[style.height]": "size()",
  },
})
export class Spinner {
  /**
   * Any CSS length. Defaults to p-progress-spinner's 100px (see above). The 4 sites that passed
   * `[style]="{ width: '50px', height: '50px' }"` become `size="50px"`.
   */
  public readonly size = input<string>("100px");

  public readonly ariaLabel = input<string>("Loading");

  /*
   * `strokeWidth="4"` (9 call sites) has NO equivalent here, on purpose: it thickened the stroke of
   * PrimeNG's SVG arc, and a Lucide glyph's stroke is baked into its path. The sweep drops it. It is
   * the only p-progress-spinner input that does not survive, and it affects nothing about layout.
   */
}
