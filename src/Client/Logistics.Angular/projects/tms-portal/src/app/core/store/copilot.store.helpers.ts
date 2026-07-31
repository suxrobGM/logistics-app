import type { AIQuotaStatusDto } from "@logistics/shared/api";

const DrawerWidthKey = "copilot.width";
export const DefaultDrawerWidth = 400;
const MinDrawerWidth = 320;
const MaxDrawerWidth = 640;

export const clampDrawerWidth = (width: number): number =>
  Math.min(MaxDrawerWidth, Math.max(MinDrawerWidth, Math.round(width)));

export const readStoredDrawerWidth = (): number => {
  const stored = Number(localStorage.getItem(DrawerWidthKey));
  return Number.isFinite(stored) && stored > 0 ? clampDrawerWidth(stored) : DefaultDrawerWidth;
};

export const persistDrawerWidth = (width: number): void =>
  localStorage.setItem(DrawerWidthKey, String(width));

export interface QuotaNotice {
  blocked: boolean;
  text: string;
}

/** Usage notice shown near the composer from 80% up; null hides it. */
export function buildQuotaNotice(quota: AIQuotaStatusDto | null): QuotaNotice | null {
  if (!quota) return null;
  if (quota.isOverQuota) {
    return {
      blocked: true,
      text:
        "Weekly AI quota exhausted" +
        (quota.resetsAt ? ` - resets ${new Date(quota.resetsAt).toLocaleDateString()}.` : "."),
    };
  }
  const percent = Math.round((quota.usagePercent ?? 0) * 100);
  return percent >= 80
    ? { blocked: false, text: `AI usage at ${percent}% of your weekly allowance.` }
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
