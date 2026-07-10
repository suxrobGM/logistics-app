/**
 * Reusable responsive utilities for UI primitive components.
 *
 * Lets a primitive accept either a single value or a per-breakpoint object
 * (MUI-style), e.g. `direction="col"` or `[direction]="{ xs: 'col', md: 'row' }"`.
 *
 * IMPORTANT: Tailwind v4's JIT scanner only detects class names that appear as
 * literal strings in source. When defining the per-breakpoint class tables a
 * component passes to `resolveResponsive`, every class string must be written
 * out literally (e.g. `"md:flex-row"`) — runtime concatenation will not work.
 */

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "2xl";

/**
 * Either a single value (applies at all breakpoints) or a per-breakpoint
 * object that overrides the value at each breakpoint (MUI-style).
 */
export type Responsive<T> = T | Partial<Record<Breakpoint, T>>;

/**
 * Per-breakpoint Tailwind class table for a single value variant.
 * Keys are the breakpoints; values are literal Tailwind classes including
 * the breakpoint prefix (xs has no prefix).
 */
export type BreakpointClassMap = Record<Breakpoint, string>;

/**
 * Class table keyed first by the variant value, then by breakpoint.
 * Example: `{ row: { xs: "flex-row", md: "md:flex-row", ... }, col: { ... } }`.
 */
export type ResponsiveClassTable<T extends string> = Record<T, BreakpointClassMap>;

export const breakpointOrder: readonly Breakpoint[] = ["xs", "sm", "md", "lg", "xl", "2xl"];

/**
 * Resolves a `Responsive<T>` input against a class table into the list of
 * Tailwind classes to apply. A single value resolves to the `xs` (unprefixed)
 * class; an object resolves to one class per provided breakpoint.
 */
export function resolveResponsive<T extends string>(
  value: Responsive<T> | null | undefined,
  classTable: ResponsiveClassTable<T>,
): string[] {
  if (value == null) return [];
  if (typeof value === "string") {
    return [classTable[value].xs];
  }
  const parts: string[] = [];
  for (const bp of breakpointOrder) {
    const v = value[bp];
    if (v != null) parts.push(classTable[v][bp]);
  }
  return parts;
}
