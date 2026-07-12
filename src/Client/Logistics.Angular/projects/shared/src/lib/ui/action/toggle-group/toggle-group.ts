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
 * A segmented single-choice control, on Helm's toggle-group.
 *
 * Selection is mandatory: this component never emits `null`. Every call site's handler is typed
 * non-nullable, so a deselect would break them. `valueChange` is typed `V`, and the Helm group is
 * pinned to `[nullable]="false"` — worth doing explicitly, because `BrnToggleGroup.nullable` defaults
 * to `true` and would otherwise emit `null` when the active segment is clicked again.
 *
 * `value` still accepts `null` — "nothing selected yet" is a legitimate input state, just not one the
 * user can navigate back to.
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

  /** Render the glyph only, with the label moved to a tooltip + `aria-label`. */
  public readonly iconOnly = input(false, { transform: booleanAttribute });

  public readonly size = input<"sm" | "default" | "lg">("default");
  public readonly ariaLabel = input<string>("");

  /**
   * Brain types `valueChange` loosely as `V | null`. `[nullable]="false"` means it should never emit
   * `null` here; this guard makes that a guarantee rather than a hope.
   */
  protected onValueChange(value: V | null): void {
    if (value === null) {
      return;
    }
    this.valueChange.emit(value);
  }
}
