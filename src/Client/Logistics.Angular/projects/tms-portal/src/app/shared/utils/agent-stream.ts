import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";

/** Where anything without a place yet sorts: an optimistic send, or a turn still running. */
export const AgentStreamTailKey = Number.MAX_SAFE_INTEGER;

/** Bucket key for a decision the server sent without a session. */
const UngroupedSession = "";

/**
 * Messages carry a server-assigned `sequence`; decisions carry only `createdAt`, off a different
 * clock. Both agent transcripts therefore order everything by sequence, so a turn's tool activity
 * stays above the reply summarizing it even when the two clocks disagree.
 */
export function messageSortKey(message: AgentMessageDto): number {
  return message.sequence ?? AgentStreamTailKey;
}

/** Lowest message sequence per session - where that session's turn begins. */
export function firstMessageSequenceBySession(
  messages: readonly AgentMessageDto[],
): Map<string, number> {
  const first = new Map<string, number>();
  for (const message of messages) {
    if (!message.sessionId) continue;
    const sortKey = messageSortKey(message);
    first.set(message.sessionId, Math.min(first.get(message.sessionId) ?? sortKey, sortKey));
  }
  return first;
}

/**
 * A session's decisions sort just before its first message. A session with no message yet - the
 * running turn - sorts at the very end, right where the "working..." indicator belongs.
 */
export function sessionSortKey(firstMessageSequence: number | undefined): number {
  return firstMessageSequence === undefined || firstMessageSequence === AgentStreamTailKey
    ? AgentStreamTailKey
    : firstMessageSequence - 0.5;
}

/** Decisions grouped by session, each bucket ascending by `createdAt`. */
export function groupDecisionsBySession(
  decisions: readonly AgentDecisionDto[],
): Map<string, AgentDecisionDto[]> {
  const bySession = new Map<string, AgentDecisionDto[]>();
  for (const decision of decisions) {
    const key = decision.sessionId ?? UngroupedSession;
    const bucket = bySession.get(key);
    if (bucket) {
      bucket.push(decision);
    } else {
      bySession.set(key, [decision]);
    }
  }
  for (const bucket of bySession.values()) {
    bucket.sort((a, b) => (a.createdAt ?? "").localeCompare(b.createdAt ?? ""));
  }
  return bySession;
}
