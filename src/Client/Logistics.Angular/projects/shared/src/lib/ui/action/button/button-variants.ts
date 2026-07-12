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
 * `ui-button`'s own cva. Deliberately NOT an extension of `hlm-button`'s `buttonVariants`: that cva
 * is imported by the `hlm-calendar*` primitives and `hlm-date-picker-trigger`, so adding intents to
 * it would push a variant table onto components that never asked for one.
 *
 * TWO RULES ABOUT ARBITRARY PROPERTIES, and both are load-bearing:
 *   1. A cell that needs a CSS variable writes the bracketed property:value form
 *      (`[background-color:var(…)]`), never a bg-/text- utility wrapping a bare `var()` — Tailwind
 *      has to infer the target property of an arbitrary value, and for a bare `var()` that is
 *      ambiguous (a background could be colour or image), so the declaration may not be emitted.
 *   2. But an arbitrary property lands in its OWN tailwind-merge class group, so it survives the
 *      merge alongside a conflicting `bg-*` / `border-*` utility instead of beating it — leaving
 *      stylesheet source order to pick the winner. So a cell that has to OVERRIDE a named utility
 *      coming from BASE (its transparent border, say) must itself be spelled as a named utility.
 *
 * Colours come from tokens defined in both `:root` and `.dark-theme` (shared/src/styles/theme.css),
 * so dark mode is free and there is no hex literal in this file.
 */

/**
 * The intent x appearance matrix, fully enumerated. The `Record<UiButtonIntent, Record<...>>` type is
 * the point: a missing cell is a compile error. It has to be, because the failure is otherwise
 * silent — an unknown Tailwind token emits no CSS and no warning, so a hole renders as an unstyled
 * button that nothing catches.
 *
 * Two product decisions that must not be "corrected" back to shadcn defaults: danger/solid is SOLID
 * red (shadcn's destructive is a soft tint) and `success` keeps green (shadcn has no success intent).
 * These sit on money-moving actions — payroll, invoices, refunds — where the colour carries meaning.
 */
export const INTENT_APPEARANCE: Record<UiButtonIntent, Record<UiButtonAppearance, string>> = {
  /**
   * The only intent that resolves through `--ui-btn-*` indirection rather than straight to a semantic
   * token: TMS paints its primary buttons with a gradient + heavier weight, the other apps run flat.
   * Each app supplies the variables (flat defaults in shared theme.css, the gradient in
   * tms-portal/src/styles.css). The gradient rides on `background-image` — a `linear-gradient()` is
   * not a valid `background-color`, so no single background utility could carry it. Only `solid`
   * reads the vars, which is what keeps the gradient off outlined/text/link.
   */
  primary: {
    solid:
      "[background-color:var(--ui-btn-primary-bg)] [background-image:var(--ui-btn-primary-image)] [color:var(--ui-btn-primary-fg)] [font-weight:var(--ui-btn-primary-weight)] hover:[background-color:var(--ui-btn-primary-bg-hover)] hover:[background-image:var(--ui-btn-primary-image-hover)]",
    outlined: "border-primary text-primary bg-transparent hover:bg-primary/10",
    text: "text-primary bg-transparent hover:bg-primary/10",
    link: "text-primary bg-transparent underline-offset-4 hover:underline",
  },

  /**
   * `--secondary` and `--accent` are the SAME token in this theme, so the obvious `hover:bg-accent`
   * would be a no-op hover. `--active` is the next step down the surface ramp and is what actually
   * produces visible feedback.
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

  /** The intent is `warn`; the TOKEN is `--warning`. The two spellings differ — keep both. */
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
   * `help` is purple, and the theme's only purple is the load-status ramp's `--status-pickedup`;
   * `--ui-btn-help` aliases it, and theme.css registers the alias as a real colour so these cells can
   * be spelled as NAMED utilities. They must be: written as arbitrary properties, the outlined
   * variant renders with no border at all (see rule 2 at the top of this file).
   */
  help: {
    solid: "bg-help [color:var(--ui-btn-on-solid)] hover:bg-help/90",
    outlined: "border-help text-help bg-transparent hover:bg-help/10",
    text: "text-help bg-transparent hover:bg-help/10",
    link: "text-help bg-transparent underline-offset-4 hover:underline",
  },

  /** `contrast`: the theme's ink and paper, swapped. Flips itself in dark mode for free. */
  contrast: {
    solid: "bg-foreground text-background hover:bg-foreground/90",
    outlined: "border-foreground text-foreground bg-transparent hover:bg-foreground/10",
    text: "text-foreground bg-transparent hover:bg-foreground/10",
    link: "text-foreground bg-transparent underline-offset-4 hover:underline",
  },
};

/**
 * The arbitrary-variant rules below size a nested `ng-icon` from the button, and they are why
 * `<ui-icon size="inherit">` (which emits no text-size class) is mandatory inside this component:
 * the variant deliberately does not match an icon that carries a text-size class of its own, so such
 * an icon stops tracking the button's size.
 *
 * Do not paraphrase those selectors in prose here. Tailwind scans this file as plain TEXT, comments
 * included, so a class-shaped string in a comment is harvested and emitted as a real CSS rule.
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
    /** Empty on purpose: the square sizing comes from the compoundVariants, which need this key. */
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
  /** `buttonClass` from the call site. Last, so a call site can win. */
  readonly extra?: string;
}

/**
 * The whole class string for the INNER `<button>`. Argument order is the precedence order — twMerge
 * lets a later group beat an earlier one, so the intent cell beats BASE, the icon-only compound beats
 * the size cell, and the call site's `extra` beats everything.
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
