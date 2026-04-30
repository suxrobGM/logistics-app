import { Component, computed, input } from "@angular/core";
import { Badge } from "../badge/badge";
import { resolveStatusSeverity, type StatusKind } from "./severity-maps";

/**
 * Domain-aware badge that auto-resolves severity from `status` + `kind`
 * (load, truck, container, subscription, invoice, employee). Replaces
 * per-page `getXxxSeverity()` methods.
 */
@Component({
  selector: "ui-status-badge",
  templateUrl: "./status-badge.html",
  imports: [Badge],
  host: { class: "contents" },
})
export class StatusBadge {
  public readonly status = input.required<string | null | undefined>();
  public readonly kind = input.required<StatusKind>();
  public readonly icon = input<string | null>(null);

  protected readonly severity = computed(() => resolveStatusSeverity(this.kind(), this.status()));

  protected readonly displayValue = computed(() => this.status() ?? "");
}
