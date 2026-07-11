import { Directive } from "@angular/core";
import { BrnTabs } from "@spartan-ng/brain/tabs";
import { classes } from "../../utils";

/**
 * VENDOR MODIFICATION: the stock generator declares `tab = input.required<string>()` here.
 *
 * That input is pure dead weight — nothing in this class reads it. The element input `tab` is
 * already forwarded to `BrnTabs.activeTab` by the `"brnTabs: tab"` host-directive alias below, so
 * the extra declaration only re-reads the same binding. Being `required`, though, it forces every
 * host to bind a raw `string`, and that is fatal for `ui-tabs`: five of the seven call sites bind a
 * NUMBER (`[value]="activeTab()"` over a `signal(0)`), and `BrnTabs` compares the active key to each
 * trigger's key with `===`. A number `0` never equals the string `"0"`, so every tab would render
 * unselected — silently, with a green build. `ui-tabs` therefore coerces the key to a string and
 * drives `BrnTabs.activeTab` (a `model`, hence settable) itself, which requires this input to not be
 * required — and, since it is unread, to not exist.
 *
 * Re-pulling this primitive with `--force tabs` will reinstate it; delete it again.
 */
/** Lifted to a const so the `ui-*` wrapper can wear these classes without composing the DIRECTIVE (NG3001). */
export const TABS_ROOT_CLASSES = "group/tabs flex gap-2 data-[orientation=horizontal]:flex-col";

@Directive({
  selector: "[hlmTabs],hlm-tabs",
  hostDirectives: [
    {
      directive: BrnTabs,
      inputs: ["orientation", "activationMode", "brnTabs: tab"],
      outputs: ["tabActivated"],
    },
  ],
  host: {
    "data-slot": "tabs",
  },
})
export class HlmTabs {
  constructor() {
    classes(() => TABS_ROOT_CLASSES);
  }
}
