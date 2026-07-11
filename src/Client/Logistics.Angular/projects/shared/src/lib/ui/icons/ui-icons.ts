import { ICON_ALIASES, type UiIconName } from "./icon-registry.generated";

/**
 * The icon runtime: name -> `@ng-icons` export name.
 *
 * `ICON_ALIASES` (generated from `tools/codemods/icon-map.json`) is the ONLY mapping table. It covers
 * both the legacy PrimeIcons-style names call sites still write (`cog`, `times`,
 * `exclamation-triangle`) and already-lucide kebab names (`chevron-down`, `settings`). Icons render
 * through `@ng-icons/lucide`, whose exports are `lucide` + PascalCase of the kebab name; the three brand
 * glyphs are hand-vendored in `./brand-icons` and exported as `brand` + PascalCase.
 *
 * Whatever a name resolves to MUST be registered via `provideIcons(...)` in the portal — see the
 * generated `BASE_NG_ICONS` / `<APP>_NG_ICONS`.
 */

/** Rendered when a name does not resolve. Registered in `BASE_NG_ICONS` for every portal. */
export const UI_ICON_ERROR_GLYPH = "lucideCircleAlert";

/**
 * Converts a kebab-case icon name to its `@ng-icons` export name.
 * `chevron-down` -> `lucideChevronDown`, `building-2` -> `lucideBuilding2`, `brand-x` -> `brandX`.
 *
 * Kept in lock-step with `toNgIconName()` in `tools/gen-icon-registry.mjs`.
 */
export function toNgIconName(kebab: string): string {
  const parts = kebab.split("-");
  const pascal = (segments: string[]) =>
    segments.map((s) => s.charAt(0).toUpperCase() + s.slice(1)).join("");

  // Lucide ships no brand icons; `brand-*` resolves to ./brand-icons instead.
  return parts[0] === "brand" ? `brand${pascal(parts.slice(1))}` : `lucide${pascal(parts)}`;
}

/**
 * Resolves an icon name to its registered `@ng-icons` export name. The single mapping site shared by
 * `ui-icon` and the nav's dynamic-icon renderers.
 *
 * An unknown name is a compile error at every static call site (see `IconName`), so this can only be
 * reached through a dynamic binding. When it is: log, and render a VISIBLE error glyph. It does NOT
 * throw — the old behaviour (pass the name through unchanged) rendered a silently blank <svg>, and a
 * throw during render would tear the view down in a zoneless app, which would make every intermediate
 * state of this migration undrivable in a browser.
 */
export function resolveNgIcon(name: string): string {
  // No prefix-stripping. S3 swept every call site onto the bare canonical name, so a `pi pi-x` spelling
  // arriving here is now a genuine BUG (a missed dynamic call site), not a legacy spelling to tolerate
  // — and it must surface as the error glyph rather than be silently normalized into working output.
  const alias = ICON_ALIASES[name as UiIconName];

  if (!alias) {
    console.error(
      `[ui-icon] Unknown icon name "${name}". Add it to tools/codemods/icon-map.json and run ` +
        `\`node tools/gen-icon-registry.mjs\`. Rendering the error glyph instead.`,
    );
    return UI_ICON_ERROR_GLYPH;
  }

  return toNgIconName(alias);
}
