/**
 * The badge/tag colour vocabulary — ONE definition for the whole workspace.
 *
 * Every `severity`-producing helper in every app returns this type. Before S5 there were FIVE names
 * for it (`Tag["severity"]`, `BadgeSeverity`, `TagSeverity`, `SeverityLevel`, `SeverityType`), four
 * of them structurally identical and one silently narrower — which is how a producer ends up handing
 * back a value the consumer cannot paint.
 *
 * =================================================================================================
 * WHY THERE IS NO `neutral` AND NO `primary` (yet) — read before adding one
 * =================================================================================================
 * These six ARE PrimeNG's `<p-tag>` vocabulary, exactly:
 *
 *     severity: 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' | undefined | null
 *                                                            — primeng/types/primeng-tag.d.ts
 *
 * Until the S5 template sweep lands, ~60 producers still feed `<p-tag [severity]="...">`. That input
 * is NOT loosely typed, so a union member p-tag does not know (`neutral`, `primary`) is not merely
 * cosmetic — it is unassignable, and `strictTemplates` fails the build at all 80 binding sites.
 * Widening a union is backwards-compatible for producers, so `primary` can be added the moment the
 * last `<p-tag>` is gone. Adding it NOW is a build break, not a head start.
 *
 * `neutral` is a different mistake: it has no `<p-tag>` colour at all. The grey chip PrimeNG paints
 * is `secondary` ({surface.100}/{surface.600}), which this union already has. Two names for one
 * colour is precisely the drift this type exists to end.
 *
 * =================================================================================================
 * WHAT A BARE `<p-tag>` ACTUALLY RENDERS — it is NOT neutral
 * =================================================================================================
 * `<p-tag>` with no severity gets no modifier class, so it falls through to the base rule:
 *
 *     .p-tag { background: dt('tag.primary.background'); color: dt('tag.primary.color'); }
 *                                                            — @primeuix/styles/dist/tag/index.mjs
 *
 * i.e. an unsevered tag is PRIMARY (the brand tint), not grey. No producer in this repo relies on
 * that — every one of them is total and returns a real severity — but the handful of bare `<p-tag/>`
 * literals in templates do, and the sweep that replaces them must map them to a `primary` intent, not
 * to `secondary` and not to a default of `info`. That is a template-sweep concern; it is written down
 * here because this is the file someone will read when they wonder what "no severity" meant.
 */
export type UiBadgeIntent = "secondary" | "success" | "info" | "warn" | "danger" | "contrast";

/** Runtime list of the vocabulary — for exhaustive `Record<UiBadgeIntent, T>` variant tables. */
export const UI_BADGE_INTENTS = [
  "secondary",
  "success",
  "info",
  "warn",
  "danger",
  "contrast",
] as const satisfies readonly UiBadgeIntent[];

/* =================================================================================================
 * `primary` — and why it lives in a SECOND type instead of in `UiBadgeIntent`.
 * =================================================================================================
 * The note above establishes that a bare `<p-tag>` renders PRIMARY, and says the template sweep must
 * map it to a `primary` intent. So why is `primary` not simply a member of `UiBadgeIntent`?
 *
 * Because until the last `<p-tag>` is gone, the producers feed BOTH components:
 *
 *   - 68 producers across 58 files are typed `UiBadgeIntent`.
 *   - While any `<p-tag [severity]="producer()">` remains, that value must satisfy p-tag's own input.
 *   - p-tag's `severity` does not accept `primary`.
 *
 * So widening `UiBadgeIntent` before the sweep lands is not a head start, it is a strictTemplates
 * error at every one of those bindings. Meanwhile `ui-badge` renders no `<p-tag>` at all, so IT can
 * paint `primary` — and it has to, because exactly one call site needs it:
 *
 *     <p-tag [value]="report.typeDisplay ?? report.type" />      ← no severity: the brand tint
 *                     tms-portal/…/safety/inspections-dashboard/inspections-dashboard.html:206
 *
 * Hence: `UiBadgeIntent` is the p-tag-safe vocabulary that PRODUCERS return, and `UiBadgeTone` is the
 * wider one that `ui-badge` RENDERS. Widening at the consumer is free — every `UiBadgeIntent` is
 * already a `UiBadgeTone`, so all 68 producers keep flowing into `<ui-badge [severity]>` untouched.
 *
 * WHEN THE LAST `<p-tag>` DIES (`bun run check:burndown` → pTags 0), this collapses to one type:
 * add `"primary"` to `UiBadgeIntent`, delete `UiBadgeTone`/`UI_BADGE_TONES`, and point the two
 * references in badge-variants.ts back at `UiBadgeIntent`. Nothing else moves.
 * =============================================================================================== */

/** What `<ui-badge>` can paint: p-tag's six, plus the `primary` a bare `<p-tag>` falls through to. */
export type UiBadgeTone = UiBadgeIntent | "primary";

/** Runtime list — drives the exhaustive `Record<UiBadgeTone, T>` cva tables and the /ui-lab matrix. */
export const UI_BADGE_TONES = [
  ...UI_BADGE_INTENTS,
  "primary",
] as const satisfies readonly UiBadgeTone[];
