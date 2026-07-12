import { booleanAttribute, Component, computed, input } from "@angular/core";
import type { IconName } from "../../icons/icons";
import { classes } from "../../primitives/utils";
import { Icon } from "../icon/icon";
import type { UiBadgeTone } from "./badge-intent";
import { uiBadgeClass, type UiBadgeSize } from "./badge-variants";

/**
 * The tag / chip. Replaces `<p-tag>` at 134 call sites (and the one `<p-chip>` — see below).
 *
 * For status-driven badges that resolve their own severity from a domain value, use
 * `<ui-status-badge>`; it wraps this one.
 *
 * =================================================================================================
 * THE `severity` DEFAULT STAYS `"info"`. Read this before "aligning" it with p-tag.
 * =================================================================================================
 * A bare `<p-tag>` renders PRIMARY (badge-intent.ts proves it from the PrimeNG source). A bare
 * `<ui-badge>` renders INFO. Those differ, and both are right, because they answer different
 * questions:
 *
 *   - p-tag's default is what the ONE severity-less `<p-tag>` in the repo paints today
 *     (inspections-dashboard.html:206). The template sweep gives that site an explicit
 *     `severity="primary"`, so p-tag's default dies with the tag and nothing inherits it.
 *   - ui-badge's default is what THIS component's existing callers have always painted. Every
 *     `<ui-badge>` that omits `severity` is info-blue on screen right now.
 *
 * Re-pointing this default at `primary` to "match p-tag" would repaint every defaulted ui-badge
 * while touching no call site and failing no test. That is the wrapper-default-drift bug this
 * migration has already hit four times. The default is this component's contract, not p-tag's.
 * =================================================================================================
 *
 * WHY THE HOST IS THE CHIP (there is no inner element)
 * `<p-tag>` puts `.p-tag` on its own host, so all 13 `class=` call sites lay out against the host
 * box. `classes()` (Helm's, from primitives/utils) writes our computed classes onto that same host
 * and twMerges the call site's `class` LAST — so `class="text-xs"` beats the `md` size cell instead
 * of racing it in stylesheet order. That is also why `class` needs no passthrough input: it already
 * lands exactly where PrimeNG put it.
 *
 * `<p-chip>` (1 site) folds in here rather than getting a component of its own. Nora's chip is a
 * rounded surface pill — `background {surface.200}, color {surface.900}, border-radius 16px` — i.e.
 * `<ui-badge severity="secondary" rounded>` to within 4px of horizontal padding. A component whose
 * only differentiator is 4px of padding is not worth owning.
 *
 * @example
 * <ui-badge value="Delivered" severity="success" />
 * <ui-badge [value]="count()" severity="danger" rounded icon="triangle-alert" />
 * <ui-badge severity="warn">Cancels at period end</ui-badge>
 */
@Component({
  selector: "ui-badge",
  templateUrl: "./badge.html",
  imports: [Icon],
})
export class Badge {
  /**
   * The chip's text. `null` / `undefined` / `""` render nothing and the projected `<ng-content>`
   * takes over — which is what the 4 `<p-tag>…</p-tag>` sites rely on.
   *
   * `undefined` is in the union because `<p-tag>` declared `value: string | undefined` and two call
   * sites reach the status through optional chaining (`load()?.status`), so they hand over
   * `LoadStatus | undefined` and have always rendered an empty chip when it is absent. Note what this
   * forces on `text()` below: a strict `value === null` check would let `undefined` through to
   * `String(undefined)` and paint a chip that literally reads "undefined".
   */
  public readonly value = input<string | number | null | undefined>(null);

  public readonly severity = input<UiBadgeTone>("info");

  public readonly size = input<UiBadgeSize>("md");

  /** PrimeNG's `[rounded]` (8 sites). Replaces `variant="outlined"` — see the note at the bottom. */
  public readonly rounded = input(false, { transform: booleanAttribute });

  /**
   * Typed against the generated `IconName` union — an unknown icon is a compile error, not a blank.
   *
   * `<p-tag [icon]>` applied its value as a CSS CLASS (`[ngClass]="icon"`), which is why the wrapper
   * this replaces built `pi pi-${name}` strings. It renders a real `<ui-icon>` now, and the 9 `[icon]`
   * call sites already hand over bare `IconName`s (phase 1 retyped them). Those icons render blank
   * between phase 1 and the template sweep — that is why the two must land together.
   *
   * `undefined` is accepted alongside `null`, and that is deliberate. "No icon" arrives here from
   * producers that spell absence the way TypeScript does — `icon?: IconName` on a lookup table
   * (truck-type-tag, load-type-tag) and `computed<IconName | undefined>` (trip-status-tag) — so
   * `IconName | null` alone REJECTS four call sites that compiled fine under `<p-tag>`, whose `icon`
   * was `string | undefined`. The alternative was to force `?? null` through four producers, i.e. to
   * make them spell absence unnaturally so this input could be narrower for no benefit. Both mean the
   * same thing, and `@if (icon(); as name)` in the template treats them identically.
   */
  public readonly icon = input<IconName | null | undefined>(null);

  protected readonly text = computed(() => {
    const value = this.value();
    // `== null` (loose) on purpose: it catches BOTH null and undefined. `=== null` would send
    // `undefined` to `String(undefined)` and render the word "undefined" in the chip.
    return value == null || value === "" ? null : String(value);
  });

  constructor() {
    // Writes onto the HOST, twMerging the call site's `class` last so a call site always wins.
    classes(() =>
      uiBadgeClass({ tone: this.severity(), size: this.size(), rounded: this.rounded() }),
    );
  }
}

/**
 * @deprecated `variant="outlined"` never rendered an outline. The old template mapped it straight to
 * p-tag's `[rounded]`, so it produced a rounded SOLID tag — the input has been a lie since it was
 * written, and nothing outside /ui-lab ever set it. Use `rounded`, which is what it actually did.
 * Neither preset has an outlined tag, so there is nothing to restore. Kept only so the type export
 * does not vanish mid-migration; delete it with the last `<p-tag>`.
 */
export type BadgeVariant = "solid" | "outlined";
