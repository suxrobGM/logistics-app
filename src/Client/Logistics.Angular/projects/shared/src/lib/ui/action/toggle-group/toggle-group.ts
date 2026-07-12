import { booleanAttribute, Component, input, output } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import { UiTooltip } from "../../overlay/tooltip/tooltip";
import { HlmToggleGroupImports } from "../../primitives/toggle-group";

/** One segment of a {@link UiToggleGroup}. */
export interface UiToggleOption<V extends string = string> {
  label: string;
  value: V;
  /** Optional glyph. Rendered beside the label, or alone when the group is `iconOnly`. */
  icon?: IconName;
  disabled?: boolean;
}

/**
 * A segmented single-choice control. Replaces `<p-selectButton>` (3 sites: the notifications filter,
 * the map layer switcher, and the payroll create-mode toggle) on spartan's Helm toggle-group.
 *
 * =================================================================================================
 * SELECTION IS MANDATORY. THIS IS A DELIBERATE DIVERGENCE FROM p-selectButton — AND A BUG FIX.
 * =================================================================================================
 * `p-selectButton` defaults `allowEmpty` to **true**, and in single-select mode it does:
 *
 *     newValue = selected ? null : optionValue;     // primeng/selectbutton, onOptionSelect()
 *
 * i.e. clicking the ALREADY-ACTIVE segment deselects it and writes `null`. None of the three call
 * sites opted out (`[allowEmpty]="false"` / `unselectable` appear nowhere), and none of them can
 * survive the `null`:
 *
 *   - notifications  `filterType = signal<FilterType>("all")` — a non-nullable signal. Deselecting
 *                    leaves no chip highlighted while the list silently shows everything.
 *   - map-controls   `setLayer(layer: MapLayerType)` — non-nullable param. The `null` was persisted
 *                    to storage and set as the active map style.
 *   - payroll-add    `onModeChange(newMode: PayrollMode)` — non-nullable param. `mode()` becomes
 *                    `null`, so neither the "single" nor the "bulk" branch renders and the form body
 *                    disappears.
 *
 * Every target is typed non-nullable, and PrimeNG could hand it `null` anyway — a latent bug the
 * type system could not see. So this component cannot emit `null`: `valueChange` is typed `V`, and
 * the Helm group is pinned to `[nullable]="false"`. That last part matters on its own, because
 * spartan's `BrnToggleGroup.nullable` ALSO defaults to `true`; leaving it unbound would have
 * reproduced the very bug this component removes.
 *
 * `value` still accepts `null` — that is "nothing selected yet", which is a legitimate INPUT state.
 * It is simply not something the user can navigate back to.
 */
@Component({
  selector: "ui-toggle-group",
  templateUrl: "./toggle-group.html",
  imports: [HlmToggleGroupImports, Icon, UiTooltip],
})
export class UiToggleGroup<V extends string = string> {
  public readonly options = input<readonly UiToggleOption<V>[]>([]);

  /** The selected value. `null` means "nothing selected yet". */
  public readonly value = input<V | null>(null);

  /** Never emits `null` — see the class note. */
  public readonly valueChange = output<V>();

  /** Render the glyph only, with the label moved to a tooltip + `aria-label` (the map switcher). */
  public readonly iconOnly = input(false, { transform: booleanAttribute });

  public readonly size = input<"sm" | "default" | "lg">("default");
  public readonly ariaLabel = input<string>("");

  /**
   * Brain types `valueChange` loosely and emits `null` on a deselect. `[nullable]="false"` means it
   * should never do so here; the guard makes that a guarantee rather than a hope, so a `null` can
   * never reach a call site whose signature forbids it.
   */
  protected onValueChange(value: V | null): void {
    if (value === null) {
      return;
    }
    this.valueChange.emit(value);
  }
}
