import { inject, Injectable, signal } from "@angular/core";
import { Api, getPendingDecisions } from "@logistics/shared/api";
import { isWriteDecision } from "@/shared/utils";

/**
 * Owns the pending-decisions count behind the sidebar badge. `refresh()` seeds it at startup; while
 * the dispatch page is open its store pushes the same count in, so the badge tracks live hub events
 * without this service polling.
 */
@Injectable({ providedIn: "root" })
export class DispatchBadgeService {
  private readonly api = inject(Api);

  public readonly pendingCount = signal(0);

  async refresh(): Promise<void> {
    try {
      const pending = await this.api.invoke(getPendingDecisions);
      this.pendingCount.set((pending ?? []).filter(isWriteDecision).length);
    } catch {
      // Silently fail - badge is non-critical
    }
  }
}
