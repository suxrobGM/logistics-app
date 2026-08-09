import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";
import {
  firstMessageSequenceBySession,
  groupDecisionsBySession,
  messageSortKey,
  sessionSortKey,
} from "@/shared/utils";

export type CopilotStreamItem = { sortKey: number } & (
  | { kind: "message"; message: AgentMessageDto }
  | { kind: "decision"; decision: AgentDecisionDto }
);

/**
 * Messages and decision cards interleaved by message sequence.
 * The dispatch page groups a session's decisions into one timeline; the drawer
 * is too narrow for that, so each decision stays its own card.
 */
export function buildCopilotStream(
  messages: readonly AgentMessageDto[],
  decisions: readonly AgentDecisionDto[],
): CopilotStreamItem[] {
  const firstSequence = firstMessageSequenceBySession(messages);

  const items: CopilotStreamItem[] = messages.map((message) => ({
    kind: "message" as const,
    sortKey: messageSortKey(message),
    message,
  }));

  for (const [sessionId, bucket] of groupDecisionsBySession(decisions)) {
    const sortKey = sessionSortKey(firstSequence.get(sessionId));
    // Ties keep insertion order, which is the bucket's createdAt order.
    for (const decision of bucket) items.push({ kind: "decision", sortKey, decision });
  }

  return items.sort((a, b) => a.sortKey - b.sortKey);
}
