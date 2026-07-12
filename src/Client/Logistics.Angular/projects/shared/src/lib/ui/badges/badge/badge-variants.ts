import { cva } from "class-variance-authority";
import { hlm } from "../../primitives/utils";
import { type UiBadgeIntent } from "./badge-intent";

export type UiBadgeSize = "sm" | "md" | "lg";

/**
 * The tone table, shared with `<ui-count-badge>` (same seven colours, different shape).
 *
 * `Record<UiBadgeIntent, string>` is exhaustive on purpose: a missing cell is a compile error,
 * because the failure is otherwise silent — an unknown Tailwind token emits no CSS and no warning,
 * so a hole would render `severity="danger"` as a transparent, unstyled chip.
 *
 * Spell every cell as a NAMED utility, never an arbitrary property (`[background-color:var(…)]`): an
 * arbitrary property is its own tailwind-merge class group, so it survives the merge alongside a call
 * site's `bg-*` instead of losing to it, leaving stylesheet source order to pick the winner.
 *
 * `--inverse` is the theme's opposing ink: a saturated fill needs its ink to flip with the theme.
 */
export const TONE_CLASSES: Record<UiBadgeIntent, string> = {
  primary: "bg-primary text-primary-foreground",

  secondary: "bg-secondary text-subtle-foreground",

  success: "bg-success text-[var(--inverse)]",

  info: "bg-info text-[var(--inverse)]",

  /** The tone is `warn`; the TOKEN is `--warning`. They differ — keep both. */
  warn: "bg-warning text-[var(--inverse)]",

  danger: "bg-danger text-[var(--inverse)]",

  contrast: "bg-foreground text-background",
};

/** `md` is the default size. Real utilities, so a call site's `class="px-4 py-2"` wins the merge. */
export const badgeVariants = cva(
  "inline-flex w-fit shrink-0 items-center justify-center gap-1 overflow-hidden whitespace-nowrap [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0",
  {
    variants: {
      size: {
        sm: "px-1.5 py-0.5 text-xs font-medium",
        md: "px-2.5 py-1 text-xs font-medium",
        lg: "px-4 py-2 text-lg font-semibold",
      },
      /**
       * NOT `rounded-xl` — that utility is a different size. `--ui-radius-*` are the shared chip
       * radii, also read by ui-progress and ui-skeleton.
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
  readonly tone: UiBadgeIntent;
  readonly size: UiBadgeSize;
  readonly rounded: boolean;
  /** The call site's `class`. Last, so a call site can win. */
  readonly extra?: string;
}

/**
 * The whole class string for the badge host. `hlm()` (twMerge) runs last, so later arguments beat
 * earlier ones and the call site's `extra` beats the size cell.
 */
export function uiBadgeClass(options: UiBadgeClassOptions): string {
  return hlm(
    badgeVariants({ size: options.size, rounded: options.rounded }),
    TONE_CLASSES[options.tone],
    options.extra,
  );
}
