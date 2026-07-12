/**
 * The badge/tag colour vocabulary — one definition for the whole workspace. Every `severity`-producing
 * helper in every app returns this type.
 *
 * There is deliberately no `neutral`: the grey chip is `secondary`.
 */
export type UiBadgeIntent =
  | "primary"
  | "secondary"
  | "success"
  | "info"
  | "warn"
  | "danger"
  | "contrast";

/** Runtime list of the vocabulary — for exhaustive `Record<UiBadgeIntent, T>` variant tables. */
export const UI_BADGE_INTENTS = [
  "primary",
  "secondary",
  "success",
  "info",
  "warn",
  "danger",
  "contrast",
] as const satisfies readonly UiBadgeIntent[];
