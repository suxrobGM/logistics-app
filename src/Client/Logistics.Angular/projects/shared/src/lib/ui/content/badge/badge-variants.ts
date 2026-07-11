import { cva } from "class-variance-authority";
import { hlm } from "../../primitives/utils";
import { type UiBadgeTone } from "./badge-intent";

export type UiBadgeSize = "sm" | "md" | "lg";

/**
 * `ui-badge`'s OWN cva — the tag/chip table. Structured to mirror `action/button/button-variants.ts`;
 * read that file first, its reasoning applies here almost line for line.
 *
 * WHY IT IS NOT BUILT ON `hlm-badge`'s cva (we generated it, then deleted it)
 * Helm's badge is shadcn's badge: `h-5 rounded-4xl px-2 py-0.5` — a fixed-height, fully-rounded pill
 * with six variants (default / secondary / destructive / outline / ghost / link). A `<p-tag>` has no
 * fixed height, is NOT a pill (2px radius in Nora, 6px in Aura), and speaks a different six-word
 * vocabulary. Adopting Helm's table would have meant overriding its height, its radius, its padding
 * and all six of its colour cells — i.e. keeping nothing — while quietly re-rounding 134 tags. So
 * this table stands alone, exactly as `buttonVariants` does. What we DID take from Helm's badge is
 * the structural half of BASE below (inline-flex / w-fit / shrink-0 / icon sizing); that is a class
 * string, not a dependency.
 *
 * COLOURS COME FROM TOKENS ONLY — and, for five of the seven, through a per-app indirection.
 * The four apps do not run the same PrimeNG preset (TMS = Nora, the rest = stock Aura), and the two
 * presets disagree about what a tag even IS:
 *
 *     Aura   .p-tag-success   background {green.100}   color {green.700}    ← soft tint
 *     Nora   .p-tag-success   background {green.600}   color {surface.0}    ← solid fill
 *
 * Neither is "the" tag. Both are shipping. So the five hue cells resolve `--ui-badge-<tone>-bg/-fg`,
 * shared/theme.css gives them Aura's recipe and tms-portal/src/styles.css gives them Nora's. Hard-
 * coding either one here would silently repaint ~130 chips in one portal or ~130 in the other three.
 */

/**
 * 7 tones, enumerated. `Record<UiBadgeTone, string>` is the point: a missing cell is a COMPILE ERROR.
 * It has to be, because the failure is otherwise silent — an unknown Tailwind token emits no CSS and
 * no warning, so a hole renders `severity="danger"` as an unstyled chip, and "the overdue-invoice tag
 * went transparent" is not something the type checker, the linter or a unit test would catch.
 *
 * ARBITRARY PROPERTIES, NOT ARBITRARY VALUES, for the five that dereference a variable — same rule
 * (and the same tailwind-merge trap) documented at length in button-variants.ts. `secondary` and
 * `contrast` do NOT need the indirection: Nora and Aura agree on both (a surface chip and the theme's
 * ink-and-paper inverted), so they are spelled as plain named utilities and merge normally.
 */
export const TONE_CLASSES: Record<UiBadgeTone, string> = {
  /** A bare `<p-tag>` — no severity — falls through to `.p-tag`, which paints `tag.primary`. */
  primary: "[background-color:var(--ui-badge-primary-bg)] [color:var(--ui-badge-primary-fg)]",

  /**
   * The grey chip. Aura says {surface.100}/{surface.600}, Nora says {surface.200}/{surface.700} —
   * one step apart on the same ramp, and `bg-secondary` / `text-subtle-foreground` lands between
   * them. Not worth a fork; it is the only cell that is an approximation rather than a reproduction.
   */
  secondary: "bg-secondary text-subtle-foreground",

  success: "[background-color:var(--ui-badge-success-bg)] [color:var(--ui-badge-success-fg)]",

  info: "[background-color:var(--ui-badge-info-bg)] [color:var(--ui-badge-info-fg)]",

  /** The tone is `warn` (PrimeNG's severity name); the TOKEN is `--warning`. They differ — keep both. */
  warn: "[background-color:var(--ui-badge-warn-bg)] [color:var(--ui-badge-warn-fg)]",

  danger: "[background-color:var(--ui-badge-danger-bg)] [color:var(--ui-badge-danger-fg)]",

  /** Identical in both presets: {surface.950} on {surface.0}, and the reverse in dark. Free flip. */
  contrast: "bg-foreground text-background",
};

/**
 * `md` is the DEFAULT and is the only size that reads the per-app variables, because it is the only
 * one the presets disagree about (Aura .875rem/700/.5rem, TMS .75rem/500/.625rem). `sm` and `lg` are
 * explicit call-site overrides — nobody's preset has an opinion about them — so they are plain named
 * utilities.
 *
 * font-size and padding are spelled as `text-[length:…]` / `px-[…]` / `py-[…]` rather than as
 * arbitrary PROPERTIES on purpose: those are real `text` / `px` / `py` utilities as far as
 * tailwind-merge is concerned, so a call site that passes `class="text-xs"` or `class="px-4 py-2"`
 * still wins the merge. Written as `[font-size:var(…)]` they would land in a different class group,
 * survive the merge alongside the call site's class, and leave the winner to stylesheet source order
 * — which is precisely the bug that shipped the borderless `help` button in S4. font-weight has no
 * such utility form and no call site sets it, so it stays an arbitrary property.
 */
export const badgeVariants = cva(
  "inline-flex w-fit shrink-0 items-center justify-center gap-1 overflow-hidden whitespace-nowrap [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0",
  {
    variants: {
      size: {
        sm: "px-1.5 py-0.5 text-xs font-medium",
        md: "text-[length:var(--ui-badge-font-size)] [font-weight:var(--ui-badge-font-weight)] px-[var(--ui-badge-px)] py-[var(--ui-badge-py)]",
        lg: "px-4 py-2 text-lg font-semibold",
      },
      /**
       * PrimeNG's `[rounded]` (8 sites): `tag.roundedBorderRadius` = `{border.radius.xl}` = 12px, in
       * BOTH presets. NOT `rounded-xl` — that utility is 16px in TMS (variables.css re-points
       * `--radius-xl` to 1rem) and 12px in the other three, so it would invent a 4px fork where
       * PrimeNG has none. `--ui-radius-pill` is 12px everywhere; see theme.css.
       */
      rounded: {
        true: "rounded-[var(--ui-radius-pill)]",
        false: "rounded-[var(--ui-radius-content)]",
      },
    },
    defaultVariants: { size: "md", rounded: false },
  },
);

export interface UiBadgeClassOptions {
  readonly tone: UiBadgeTone;
  readonly size: UiBadgeSize;
  readonly rounded: boolean;
  /** The call site's `class` (PrimeNG's `styleClass`). Last, so a call site can win. */
  readonly extra?: string;
}

/**
 * The whole class string for the badge host. `hlm()` (twMerge) runs last so later groups beat earlier
 * ones: the tone cell's colours beat nothing in BASE, the call site's `extra` beats the size cell.
 */
export function uiBadgeClass(options: UiBadgeClassOptions): string {
  return hlm(
    badgeVariants({ size: options.size, rounded: options.rounded }),
    TONE_CLASSES[options.tone],
    options.extra,
  );
}
