import type { IconName } from "../../icons/icons";

/**
 * A single entry in a `<ui-menu>`. Repo-owned replacement for PrimeNG's `MenuItem`
 * (`primeng/api`), which was imported by 18 feature files.
 *
 * TWO DELIBERATE DIVERGENCES FROM `MenuItem`, both of which turn a silent runtime failure into a
 * compile error:
 *
 *   1. `icon` is an {@link IconName}, not a `pi pi-*` CSS class string. PrimeNG rendered
 *      `icon: "pi pi-eyeee"` as an empty `<span>`; here a name outside the union does not compile.
 *      It also feeds `<ui-icon [name]>`, so the glyph is registered by the icon generator
 *      (`icon: "eye"` is matched by its `RE_ICON_PROP` scan) instead of relying on a global CSS font.
 *
 *   2. `variant: "destructive"` replaces `styleClass: "text-red-600"`. All five `styleClass` users
 *      were "Delete" rows hardcoding a red — two different reds, in fact (`text-red-600` x4,
 *      `text-red-500` x1) for one meaning, which the no-hardcoded-colours rule forbids. Helm's item
 *      already ships a themed `data-[variant=destructive]` state that tints the icon and the hover
 *      background too, neither of which the bare text colour did.
 *
 * NOT MODELLED, because nothing uses them (verified across all 19 files, not assumed from the brief,
 * which claimed both were in play): nested `items` submenus — the four `items` hits are all local
 * `const items: MenuItem[]` declarations — and `url`, of which there are zero. Add them when a real
 * call site needs them.
 */
export interface UiMenuItem {
  label?: string;
  icon?: IconName;
  /** Runs on click and on keyboard Enter/Space. The menu closes immediately afterwards. */
  command?: () => void;
  /** Rendered as a real `<a>` so ctrl/middle-click and "open in new tab" keep working. */
  routerLink?: string | readonly unknown[];
  disabled?: boolean;
  /** `false` removes the item. Mirrors `MenuItem.visible`; `undefined` means visible. */
  visible?: boolean;
  /** A horizontal rule. Every other field is ignored on a separator. */
  separator?: boolean;
  /** `destructive` themes the row as a danger action (was `styleClass: "text-red-600"`). */
  variant?: "default" | "destructive";
}
