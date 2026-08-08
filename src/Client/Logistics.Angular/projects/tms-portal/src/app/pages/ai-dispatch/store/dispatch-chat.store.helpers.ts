import type { AIQuotaStatusDto } from "@logistics/shared/api";
import { DateUtils } from "@logistics/shared/utils";

const RightPanelCollapsedKey = "dispatch-chat.right-panel-collapsed";

export const readStoredRightPanelCollapsed = (): boolean =>
  localStorage.getItem(RightPanelCollapsedKey) === "true";

export const persistRightPanelCollapsed = (collapsed: boolean): void =>
  localStorage.setItem(RightPanelCollapsedKey, String(collapsed));

export interface QuotaNotice {
  /** "blocked" = hard pause (composer disables); "overage" = billing through; "info" = nearing budget. */
  severity: "info" | "overage" | "blocked";
  text: string;
}

/**
 * Usage notice shown near the composer from 80% up; null hides it. Deliberate near-copy of
 * `CopilotStore`'s `buildQuotaNotice` - keep the two in sync.
 */
export function buildQuotaNotice(
  quota: AIQuotaStatusDto | null,
  formatCurrency: (value: number) => string,
): QuotaNotice | null {
  if (!quota) return null;
  const until = quota.resetsAt ? ` until ${DateUtils.toLocaleDate(quota.resetsAt)}.` : ".";

  if (quota.overageBlocked) {
    return { severity: "blocked", text: `Weekly AI budget reached - AI is paused${until}` };
  }
  if (quota.isOverQuota) {
    const accrued =
      (quota.overageChargesUsd ?? 0) > 0
        ? ` - ${formatCurrency(quota.overageChargesUsd!)} so far this week`
        : "";
    return {
      severity: "overage",
      text: `Weekly AI allowance used - further messages are billed as overage${accrued}${until}`,
    };
  }
  const percent = Math.round((quota.usagePercent ?? 0) * 100);
  return percent >= 80
    ? { severity: "info", text: `AI usage at ${percent}% of your weekly allowance.` }
    : null;
}

/** Safety net for turns whose terminal hub event never arrives (hub down, job silently skipped). */
const TurnPollIntervalMs = 45_000;
const LongRunningAfterMs = 180_000;
const LongRunningAfterTicks = Math.ceil(LongRunningAfterMs / TurnPollIntervalMs);

/** One interval per active turn: reconciles every tick, flags long-running once past the threshold. */
export class TurnWatchdog {
  private pollHandle: ReturnType<typeof setInterval> | null = null;
  private ticks = 0;

  constructor(
    private readonly onPoll: () => void,
    private readonly onLongRunning: () => void,
  ) {}

  start(): void {
    this.pollHandle ??= setInterval(() => {
      this.ticks++;
      if (this.ticks === LongRunningAfterTicks) {
        this.onLongRunning();
      }
      this.onPoll();
    }, TurnPollIntervalMs);
  }

  stop(): void {
    if (this.pollHandle !== null) {
      clearInterval(this.pollHandle);
      this.pollHandle = null;
      this.ticks = 0;
    }
  }
}
