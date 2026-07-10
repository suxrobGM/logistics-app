import { Component, computed, input } from "@angular/core";
import { Badge, type BadgeSeverity } from "../badge/badge";
import { resolveStatusSeverity, type StatusKind } from "./severity-maps";

const SEVERITY_DEFAULT_ICON: Partial<Record<BadgeSeverity, string>> = {
  success: "check-circle",
  danger: "times-circle",
  warn: "exclamation-triangle",
  info: "info-circle",
};

/**
 * Domain-aware badge that auto-resolves severity from `status` + `kind`
 * (load, truck, container, subscription, invoice, employee). Replaces
 * per-page `getXxxSeverity()` methods.
 *
 * Icons: callers can pass an explicit `icon`; otherwise a sensible default
 * is chosen from the resolved severity (success → check-circle, etc.).
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
  public readonly icon = input<string | null>(null);

  protected readonly severity = computed(() => resolveStatusSeverity(this.kind(), this.status()));

  protected readonly displayValue = computed(() => this.status() ?? "");

  protected readonly resolvedIcon = computed(() => {
    const explicit = this.icon();
    if (explicit !== null) return explicit;
    return SEVERITY_DEFAULT_ICON[this.severity()] ?? null;
  });
}
