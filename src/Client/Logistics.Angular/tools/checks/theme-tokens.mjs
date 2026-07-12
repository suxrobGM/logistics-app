#!/usr/bin/env node
/**
 * CI gate: no hardcoded palette colours in the app portals or the shared UI library.
 *
 * `.claude/rules/frontend/angular-conventions.md` has said "never use hardcoded color values" for a
 * long time, and 88 violations shipped anyway — because nothing checked. A hardcoded colour does not
 * follow the theme, so it survives every build, test and lint run and then renders a white sidebar on
 * a dark page. That is exactly how admin/customer dark mode broke.
 *
 * Use a semantic token instead — they flip themselves:
 *   bg-white / bg-gray-50   -> bg-card / bg-background / bg-sidebar
 *   text-gray-600           -> text-muted-foreground
 *   border-gray-200         -> border-border
 *   text-green-600          -> text-success   (also: warning / danger / info)
 *   bg-green-100 (a tint)   -> bg-success/15
 *
 * SCOPE — admin + customer only, for now.
 *
 * These two are the portals that broke, and the reason is specific: they carried hardcoded colours
 * with NO `dark:` counterpart at all, so nothing flipped. They are clean now, and this gate keeps them
 * that way.
 *
 * `tms-portal` and `shared/ui` still hold ~325 palette classes, but nearly all are explicit
 * `text-gray-600 dark:text-gray-400` PAIRS — off-token, yet they do flip, so they are a tidy-up and not
 * a bug. Converting them would repaint TMS, which is deliberately being kept pixel-identical. Widen the
 * scope when that repaint is in scope; do not widen it silently, or this gate gets disabled instead.
 *
 * The website is EXEMPT permanently: it runs its own editorial palette by design and has no dark mode.
 *
 * The scan/report/CLI shell lives in ../lib/check.mjs (shared with every other gate).
 *
 *   node tools/checks/theme-tokens.mjs
 *   node tools/checks/theme-tokens.mjs --self-test
 */
import { cli, defineCheck } from "../lib/check.mjs";

const PALETTE =
  "red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|slate|gray|zinc|neutral|stone";
const PROPERTY = "bg|text|border|from|to|via|ring|divide|outline|shadow|fill|stroke|accent|caret";

/**
 * `surface-*` is deliberately NOT flagged: it is a real registered ramp used with explicit light/dark
 * shades (`bg-surface-300 dark:bg-surface-600`). It is a footgun, not a bug.
 */
const RULE = new RegExp(
  String.raw`(?<![\w-])(?:${PROPERTY})-(?:(?:${PALETTE})-\d{2,3}|white|black)(?![\w-])`,
  "g",
);

export function findHardcodedColors(src) {
  return [...src.matchAll(RULE)].map((m) => ({ token: m[0], offset: m.index }));
}

export const check = defineCheck({
  title: "Theme tokens (no hardcoded palette colours)",
  dirs: ["projects/admin-portal/src", "projects/customer-portal/src"],
  ext: [".html", ".ts"],
  /** Specs assert on arbitrary class strings (`bg-purple-500`); they render nothing. */
  exempt: /\/primitives\/|\.spec\.ts$/,
  clean: "no hardcoded palette colours",
  violation: "hardcoded colour(s)",
  hint: "Use a semantic token:",
  find: findHardcodedColors,
  cases: [
    ['<div class="bg-gray-50">', 1, "bg-gray-50"],
    ['<div class="bg-white">', 1, "bg-white"],
    ['<p class="text-gray-600">', 1, "text-gray-600"],
    ['<i class="text-green-600">', 1, "semantic palette colour"],
    ['<div class="hover:bg-gray-100">', 1, "behind a variant"],
    ['<div class="bg-background text-muted-foreground">', 0, "semantic tokens"],
    ['<div class="bg-success/15 border-border">', 0, "token with opacity"],
    ['<div class="bg-surface-300 dark:bg-surface-600">', 0, "the surface-* ramp is allowed"],
    ['<div class="grid-cols-3 gap-2 p-4">', 0, "non-colour utilities"],
    ['<div class="bg-primary/12">', 0, "primary token"],
  ],
});

cli(import.meta.url, check);
