/** What a call site may bind to `<ui-tabs [value]>` — both are accepted because five sites bind numbers. */
export type UiTabValue = string | number;

/**
 * Brain keys tabs by strict string identity (`$activeTab() === triggerFor()`), so the active value has
 * to be normalised before it reaches `BrnTabs`. `ui-tab` / `ui-tab-panel` take their key as a plain
 * string attribute already, which is the other half of the same contract: without this, a `signal(0)`
 * call site would compare `0 === "0"`, select no tab at all, and render a blank panel area silently.
 */
export const tabKey = (value: UiTabValue | undefined): string => String(value ?? "");
