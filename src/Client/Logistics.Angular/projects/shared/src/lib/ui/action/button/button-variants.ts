import { cva } from "class-variance-authority";
import { hlm } from "../../primitives/utils";

export type UiButtonIntent =
  | "primary"
  | "secondary"
  | "success"
  | "info"
  | "warn"
  | "danger"
  | "help"
  | "contrast";

export type UiButtonAppearance = "solid" | "outlined" | "text" | "link";

export type UiButtonSize = "sm" | "md" | "lg";

/**
 * `ui-button`'s OWN cva. It is deliberately NOT an extension of `hlm-button`'s `buttonVariants`:
 * that cva is imported by the four `hlm-calendar*` primitives and by `hlm-date-picker-trigger`, so
 * adding intents to it would re-render the calendar nav and the date-picker trigger through a
 * variant table they never asked for. Zero forks of vendored Helm — this table stands alone.
 *
 * COLOURS COME FROM TOKENS ONLY. Every value below dereferences a custom property that is defined
 * in BOTH `:root` and `.dark-theme` (see shared/src/styles/theme.css), so dark mode is free and
 * there is not one hex literal in this file.
 *
 * ARBITRARY PROPERTIES, NOT ARBITRARY VALUES. Where a cell needs a CSS variable it is written in
 * the bracketed property:value form, never as a bg-/text- utility wrapping a bare var(). Tailwind
 * has to *infer* the target property of an arbitrary value, and for a bare var() that inference is
 * ambiguous — a background could be a colour or an image, a text- could be a colour or a font-size.
 * Naming the property outright means the declaration is always emitted.
 */

/**
 * 8 intents x 4 appearances = 32 cells, enumerated. The `Record<UiButtonIntent, Record<...>>` type
 * is the point: a missing cell is a COMPILE ERROR. It has to be, because every other failure mode
 * here is silent — an unknown Tailwind token emits no CSS and no warning, so a hole would render
 * `intent="success"` as an unstyled (or primary-blue) button, and "the payroll button turned blue"
 * is not something a type checker, a linter or a unit test would otherwise catch.
 *
 * Two product decisions are encoded here and must not be "corrected" back to shadcn defaults:
 *   danger/solid  renders SOLID RED (shadcn's destructive is a soft tint), and
 *   success       KEEPS GREEN (shadcn has no success intent at all).
 * These sit on money-moving actions — payroll, invoices, subscriptions, void, refund — where the
 * colour carries the meaning. Collapsing them into the primary blue changes what the button says.
 */
export const INTENT_APPEARANCE: Record<UiButtonIntent, Record<UiButtonAppearance, string>> = {
  /**
   * The only intent that resolves through `--ui-btn-*` indirection rather than straight to a
   * semantic token. TMS paints its primary buttons with a gradient + font-weight 600 (its
   * `primeng-preset.ts` did this to `.p-button-primary`); admin / customer / website run stock and
   * must stay pixel-identical. So the cell reads the variables and each app supplies them: flat
   * defaults in shared theme.css, the gradient in tms-portal/src/styles.css. The gradient rides on
   * `background-image` — a `linear-gradient()` is not a valid `background-color`, so a single
   * `bg-*` utility could never have carried it.
   *
   * Note TMS's preset scoped the gradient to `:not(.p-button-text):not(.p-button-outlined)
   * :not(.p-button-link)`. That exclusion is reproduced structurally: only `solid` reads the vars.
   */
  primary: {
    solid:
      "[background-color:var(--ui-btn-primary-bg)] [background-image:var(--ui-btn-primary-image)] [color:var(--ui-btn-primary-fg)] [font-weight:var(--ui-btn-primary-weight)] hover:[background-color:var(--ui-btn-primary-bg-hover)] hover:[background-image:var(--ui-btn-primary-image-hover)]",
    outlined: "border-primary text-primary bg-transparent hover:bg-primary/10",
    text: "text-primary bg-transparent hover:bg-primary/10",
    link: "text-primary bg-transparent underline-offset-4 hover:underline",
  },

  /**
   * `--secondary` and `--accent` are the SAME token (`--bg-hover`) in this theme, so the obvious
   * `hover:bg-accent` would be a no-op hover. `--active` (`--bg-active`) is the next step down the
   * surface ramp and is what actually produces visible feedback.
   */
  secondary: {
    solid: "bg-secondary text-secondary-foreground hover:bg-active",
    outlined:
      "border-strong text-subtle-foreground bg-transparent hover:bg-secondary hover:text-foreground",
    text: "text-subtle-foreground bg-transparent hover:bg-secondary hover:text-foreground",
    link: "text-subtle-foreground bg-transparent underline-offset-4 hover:text-foreground hover:underline",
  },

  success: {
    solid: "bg-success [color:var(--ui-btn-on-solid)] hover:bg-success/90",
    outlined: "border-success text-success bg-transparent hover:bg-success/10",
    text: "text-success bg-transparent hover:bg-success/10",
    link: "text-success bg-transparent underline-offset-4 hover:underline",
  },

  info: {
    solid: "bg-info [color:var(--ui-btn-on-solid)] hover:bg-info/90",
    outlined: "border-info text-info bg-transparent hover:bg-info/10",
    text: "text-info bg-transparent hover:bg-info/10",
    link: "text-info bg-transparent underline-offset-4 hover:underline",
  },

  /** The intent is `warn` (PrimeNG's severity name); the TOKEN is `--warning`. They differ — keep both. */
  warn: {
    solid: "bg-warning [color:var(--ui-btn-on-solid)] hover:bg-warning/90",
    outlined: "border-warning text-warning bg-transparent hover:bg-warning/10",
    text: "text-warning bg-transparent hover:bg-warning/10",
    link: "text-warning bg-transparent underline-offset-4 hover:underline",
  },

  danger: {
    solid: "bg-danger [color:var(--ui-btn-on-solid)] hover:bg-danger/90",
    outlined: "border-danger text-danger bg-transparent hover:bg-danger/10",
    text: "text-danger bg-transparent hover:bg-danger/10",
    link: "text-danger bg-transparent underline-offset-4 hover:underline",
  },

  /**
   * PrimeNG's `help` severity is purple and this theme has exactly one purple defined in both
   * `:root` and `.dark-theme`: the load-status ramp's `--status-pickedup`. `--ui-btn-help` aliases
   * it rather than introducing a ninth hex literal into the palette, and theme.css registers that
   * alias as a real colour so the cells below can be spelled as NAMED utilities.
   *
   * They must be named. An earlier version wrote this cell with arbitrary properties, and the
   * outlined variant shipped with NO BORDER: tailwind-merge files an arbitrary property under a
   * different class group than the equivalent named utility, so it never saw the cell's border
   * colour as conflicting with BASE's `border-transparent`. Both classes survived the merge, they
   * have equal specificity, and the base one happens to come later in the emitted stylesheet — so
   * it won. The other seven intents were never exposed to this because they were already named.
   */
  help: {
    solid: "bg-help [color:var(--ui-btn-on-solid)] hover:bg-help/90",
    outlined: "border-help text-help bg-transparent hover:bg-help/10",
    text: "text-help bg-transparent hover:bg-help/10",
    link: "text-help bg-transparent underline-offset-4 hover:underline",
  },

  /** PrimeNG's `contrast`: the theme's ink and paper, swapped. Flips itself in dark mode for free. */
  contrast: {
    solid: "bg-foreground text-background hover:bg-foreground/90",
    outlined: "border-foreground text-foreground bg-transparent hover:bg-foreground/10",
    text: "text-foreground bg-transparent hover:bg-foreground/10",
    link: "text-foreground bg-transparent underline-offset-4 hover:underline",
  },
};

/**
 * The arbitrary-variant rules below size a nested `ng-icon` from the button, and they are the reason
 * `<ui-icon size="inherit">` (which emits NO `text-*` class) is mandatory inside this component: the
 * variant deliberately does not match an icon that carries a text-size class of its own, so an icon
 * with one stops tracking the button's size. Same mechanism Helm's button uses. See ui/content/icon.
 *
 * Do not paraphrase those selectors in prose here. Tailwind scans this file as plain TEXT, comments
 * included, so a class-shaped string in a comment is harvested and emitted as a real CSS rule — an
 * earlier draft of this very comment shipped a `font-size:...` declaration into all four bundles.
 */
const BASE =
  "group/button inline-flex shrink-0 cursor-pointer items-center justify-center gap-2 rounded-md border border-transparent bg-clip-padding font-medium whitespace-nowrap transition-all outline-none select-none focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px] disabled:pointer-events-none disabled:opacity-50 [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0 [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(4)]";

export const buttonVariants = cva(BASE, {
  variants: {
    size: {
      sm: "h-8 gap-1.5 px-3 text-sm [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(3.5)]",
      md: "h-9 gap-2 px-4 text-sm",
      lg: "h-10 gap-2 px-5 text-base [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(5)]",
    },
    /** No label: the button collapses to a square so the glyph is optically centred. */
    iconOnly: { true: "", false: "" },
    rounded: { true: "rounded-full", false: "" },
  },
  compoundVariants: [
    { size: "sm", iconOnly: true, class: "size-8 p-0" },
    { size: "md", iconOnly: true, class: "size-9 p-0" },
    { size: "lg", iconOnly: true, class: "size-10 p-0" },
  ],
  defaultVariants: { size: "md", iconOnly: false, rounded: false },
});

export interface UiButtonClassOptions {
  readonly intent: UiButtonIntent;
  readonly appearance: UiButtonAppearance;
  readonly size: UiButtonSize;
  readonly iconOnly: boolean;
  readonly rounded: boolean;
  readonly block: boolean;
  /** `buttonClass` from the call site (PrimeNG's `styleClass`). Last, so a call site can win. */
  readonly extra?: string;
}

/**
 * The whole class string for the INNER `<button>`. `hlm()` (twMerge) runs last so later groups beat
 * earlier ones: the intent cell's `border-danger` beats BASE's `border-transparent`, the icon-only
 * compound's `size-9 p-0` beats the size cell's `h-9 px-4`, and the call site's `extra` beats us.
 */
export function uiButtonClass(options: UiButtonClassOptions): string {
  return hlm(
    buttonVariants({
      size: options.size,
      iconOnly: options.iconOnly,
      rounded: options.rounded,
    }),
    INTENT_APPEARANCE[options.intent][options.appearance],
    options.block && "w-full",
    options.extra,
  );
}
