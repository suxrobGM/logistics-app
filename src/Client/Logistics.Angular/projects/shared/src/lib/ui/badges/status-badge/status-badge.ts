import { Component, computed, input } from "@angular/core";
import type { IconName } from "../../icons/icons";
import { Badge } from "../badge/badge";
import type { UiBadgeIntent } from "../badge/badge-intent";
import { resolveStatusSeverity, type StatusKind } from "./severity-maps";

/** Typed against `IconName` now that `ui-badge` renders a real `<ui-icon>` — an unknown key here is
 *  a compile error rather than a chip with a blank square where its glyph should be. */
const SEVERITY_DEFAULT_ICON: Partial<Record<UiBadgeIntent, IconName>> = {
  success: "circle-check",
  danger: "circle-x",
  warn: "triangle-alert",
  info: "info",
};

/**
 * Domain-aware badge that auto-resolves severity from `status` + `kind`
 * (load, truck, container, subscription, invoice, employee). Replaces
 * per-page `getXxxSeverity()` methods.
 *
 * Icons: callers can pass an explicit `icon`; otherwise a sensible default
 * is chosen from the resolved severity (success → circle-check, etc.).
 * Pass `icon=""` (empty string) to suppress all icons.
 */
@Component({
  selector: "ui-status-badge",
  templateUrl: "./status-badge.html",
  imports: [Badge],
})
export class StatusBadge {
  public readonly status = input.required<string | null | undefined>();
  public readonly kind = input.required<StatusKind>();

  /**
   * `IconName`, not `string` — `ui-badge` now renders a real `<ui-icon>`, whose name is a checked
   * union. `""` stays in the union on purpose: it is this component's documented "no icon at all"
   * escape hatch (see the class doc), and dropping it to tidy the type would silently delete a
   * public behaviour. It has no callers today, which is exactly why nothing would have caught it.
   */
  public readonly icon = input<IconName | "" | null>(null);

  protected readonly severity = computed(() => resolveStatusSeverity(this.kind(), this.status()));

  protected readonly displayValue = computed(() => this.status() ?? "");

  protected readonly resolvedIcon = computed((): IconName | null => {
    const explicit = this.icon();
    // `""` suppresses; `null` (the default) falls through to the severity's icon.
    if (explicit === "") return null;
    if (explicit !== null) return explicit;
    return SEVERITY_DEFAULT_ICON[this.severity()] ?? null;
  });
}
