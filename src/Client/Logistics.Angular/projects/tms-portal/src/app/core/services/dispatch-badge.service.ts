import { Injectable, inject, signal } from "@angular/core";
import { Api, type DispatchDecisionDto, getPendingDecisions } from "@logistics/shared/api";

/**
 * Lightweight global service that tracks the pending dispatch decisions count
 * for the sidebar badge. Call `refresh()` after any change to pending decisions.
 */
@Injectable({ providedIn: "root" })
export class DispatchBadgeService {
  private readonly api = inject(Api);

  public readonly pendingCount = signal(0);

  async refresh(): Promise<void> {
    try {
      const pending = await this.api.invoke(getPendingDecisions);
      const writeDecisions = (pending ?? []).filter(
        (d: DispatchDecisionDto) => d.type !== "query",
      );
      this.pendingCount.set(writeDecisions.length);
    } catch {
      // Silently fail — badge is non-critical
    }
  }
}
