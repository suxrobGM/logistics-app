import type { AIQuotaStatusDto } from "@logistics/shared/api";
import { DateUtils } from "@logistics/shared/utils";
import { buildQuotaNotice, TurnWatchdog } from "./agent-chat.helpers";

const formatCurrency = (value: number): string => `$${value.toFixed(2)}`;

function quota(overrides: Partial<AIQuotaStatusDto> = {}): AIQuotaStatusDto {
  return { usagePercent: 0, ...overrides };
}

describe("buildQuotaNotice", () => {
  it("returns null when there is no quota", () => {
    expect(buildQuotaNotice(null, formatCurrency)).toBeNull();
  });

  it("blocked severity when overageBlocked, with the reset date appended", () => {
    const resetsAt = "2026-09-01T00:00:00Z";
    const notice = buildQuotaNotice(quota({ overageBlocked: true, resetsAt }), formatCurrency);

    expect(notice?.severity).toBe("blocked");
    expect(notice?.text).toBe(
      `Weekly AI budget reached - AI is paused until ${DateUtils.toLocaleDate(resetsAt)}.`,
    );
  });

  it("blocked severity ends with a bare period when there is no reset date", () => {
    const notice = buildQuotaNotice(
      quota({ overageBlocked: true, resetsAt: null }),
      formatCurrency,
    );

    expect(notice?.text).toBe("Weekly AI budget reached - AI is paused.");
  });

  it("overageBlocked takes priority over isOverQuota", () => {
    const notice = buildQuotaNotice(
      quota({ overageBlocked: true, isOverQuota: true }),
      formatCurrency,
    );

    expect(notice?.severity).toBe("blocked");
  });

  it("overage severity with accrued charges, formatted through the caller's currency formatter", () => {
    const resetsAt = "2026-09-01T00:00:00Z";
    const notice = buildQuotaNotice(
      quota({ isOverQuota: true, overageChargesUsd: 12.5, resetsAt }),
      formatCurrency,
    );

    expect(notice?.severity).toBe("overage");
    expect(notice?.text).toBe(
      `Weekly AI allowance used - further messages are billed as overage - $12.50 so far this week until ${DateUtils.toLocaleDate(resetsAt)}.`,
    );
  });

  it("overage severity omits the accrued clause when charges are zero or absent", () => {
    const withZero = buildQuotaNotice(
      quota({ isOverQuota: true, overageChargesUsd: 0 }),
      formatCurrency,
    );
    const withUndefined = buildQuotaNotice(quota({ isOverQuota: true }), formatCurrency);

    expect(withZero?.text).toBe(
      "Weekly AI allowance used - further messages are billed as overage.",
    );
    expect(withUndefined?.text).toBe(
      "Weekly AI allowance used - further messages are billed as overage.",
    );
  });

  it("info severity from 80% usage upward, with the percentage rounded", () => {
    const atThreshold = buildQuotaNotice(quota({ usagePercent: 0.799 }), formatCurrency);
    const wellOver = buildQuotaNotice(quota({ usagePercent: 0.93 }), formatCurrency);

    expect(atThreshold?.severity).toBe("info");
    expect(atThreshold?.text).toBe("AI usage at 80% of your weekly allowance.");
    expect(wellOver?.text).toBe("AI usage at 93% of your weekly allowance.");
  });

  it("returns null just below the 80% threshold", () => {
    expect(buildQuotaNotice(quota({ usagePercent: 0.794 }), formatCurrency)).toBeNull();
  });

  it("returns null when usagePercent is absent and nothing else applies", () => {
    expect(buildQuotaNotice(quota({ usagePercent: undefined }), formatCurrency)).toBeNull();
  });
});

describe("TurnWatchdog", () => {
  const PollIntervalMs = 45_000;
  const LongRunningAfterMs = 180_000;

  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("polls on every tick and does not flag long-running before the threshold", () => {
    const onPoll = vi.fn();
    const onLongRunning = vi.fn();
    const watchdog = new TurnWatchdog(onPoll, onLongRunning);

    watchdog.start();
    vi.advanceTimersByTime(PollIntervalMs * 3);

    expect(onPoll).toHaveBeenCalledTimes(3);
    expect(onLongRunning).not.toHaveBeenCalled();
  });

  it("flags long-running exactly once it crosses the threshold, and never again on later ticks", () => {
    const onPoll = vi.fn();
    const onLongRunning = vi.fn();
    const watchdog = new TurnWatchdog(onPoll, onLongRunning);

    watchdog.start();
    vi.advanceTimersByTime(LongRunningAfterMs);
    expect(onLongRunning).toHaveBeenCalledTimes(1);
    expect(onPoll).toHaveBeenCalledTimes(4);

    vi.advanceTimersByTime(PollIntervalMs * 3);
    expect(onLongRunning).toHaveBeenCalledTimes(1);
    expect(onPoll).toHaveBeenCalledTimes(7);
  });

  it("stop() clears the interval so no further polls fire", () => {
    const onPoll = vi.fn();
    const watchdog = new TurnWatchdog(onPoll, vi.fn());

    watchdog.start();
    vi.advanceTimersByTime(PollIntervalMs);
    watchdog.stop();
    vi.advanceTimersByTime(PollIntervalMs * 5);

    expect(onPoll).toHaveBeenCalledTimes(1);
  });

  it("restarting after stop() resets the tick count, so long-running takes a fresh threshold", () => {
    const onLongRunning = vi.fn();
    const watchdog = new TurnWatchdog(vi.fn(), onLongRunning);

    watchdog.start();
    vi.advanceTimersByTime(PollIntervalMs * 2);
    watchdog.stop();
    watchdog.start();
    vi.advanceTimersByTime(PollIntervalMs * 2);

    expect(onLongRunning).not.toHaveBeenCalled();

    vi.advanceTimersByTime(PollIntervalMs * 2);
    expect(onLongRunning).toHaveBeenCalledTimes(1);
  });

  it("calling start() twice without stop() does not create a second interval", () => {
    const onPoll = vi.fn();
    const watchdog = new TurnWatchdog(onPoll, vi.fn());

    watchdog.start();
    watchdog.start();
    vi.advanceTimersByTime(PollIntervalMs);

    expect(onPoll).toHaveBeenCalledTimes(1);
  });
});
