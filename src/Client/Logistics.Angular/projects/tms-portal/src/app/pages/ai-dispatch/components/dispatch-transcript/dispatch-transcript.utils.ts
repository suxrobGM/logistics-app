import type { AgentDecisionDto, AgentMessageDto, AgentSessionDto } from "@logistics/shared/api";
import {
  firstMessageSequenceBySession,
  groupDecisionsBySession,
  messageSortKey,
  sessionSortKey,
} from "@/shared/utils";

/** One agent turn's tool activity, grouped by session and rendered as a `ui-timeline`. */
export interface TranscriptTurn {
  readonly kind: "turn";
  readonly session: AgentSessionDto;
  /** Sorted ascending by `createdAt`, per the turn-grouping contract. */
  readonly decisions: AgentDecisionDto[];
  readonly sortKey: number;
}

export interface TranscriptMessage {
  readonly kind: "message";
  readonly message: AgentMessageDto;
  readonly sortKey: number;
  /** The turn's closing assistant message - rendered as the dispatch report card. */
  readonly isReport: boolean;
  readonly session?: AgentSessionDto;
}

export type TranscriptItem = TranscriptTurn | TranscriptMessage;

/**
 * Merges messages and per-turn tool-activity timelines into one stream ordered by message sequence
 * (see `agent-stream.ts`):
 * - A session with no decisions renders no timeline - nothing to show.
 * - A session's last assistant message is its report only when the turn ran tools; a tool-free
 *   (conversational) turn renders as a plain chat message. A message with no `sessionId` never is.
 */
export function buildTranscriptStream(
  messages: readonly AgentMessageDto[],
  decisions: readonly AgentDecisionDto[],
  sessions: readonly AgentSessionDto[],
): TranscriptItem[] {
  const decisionsBySession = groupDecisionsBySession(decisions);
  const messageItems: TranscriptMessage[] = [];
  const reportIndexBySession = new Map<string, number>();

  for (const message of messages) {
    const sortKey = messageSortKey(message);
    const index = messageItems.push({ kind: "message", message, sortKey, isReport: false }) - 1;
    if (
      message.sessionId &&
      message.role === "assistant" &&
      decisionsBySession.has(message.sessionId)
    ) {
      reportIndexBySession.set(message.sessionId, index);
    }
  }

  const sessionById = new Map(sessions.filter((s) => s.id).map((s) => [s.id!, s]));
  for (const [sessionId, index] of reportIndexBySession) {
    messageItems[index] = {
      ...messageItems[index],
      isReport: true,
      session: sessionById.get(sessionId),
    };
  }

  const firstSequence = firstMessageSequenceBySession(messages);

  const turnItems: TranscriptTurn[] = [];
  for (const session of sessions) {
    const sessionDecisions = session.id ? decisionsBySession.get(session.id) : undefined;
    if (!sessionDecisions?.length) continue;

    turnItems.push({
      kind: "turn",
      session,
      decisions: sessionDecisions,
      sortKey: sessionSortKey(firstSequence.get(session.id!)),
    });
  }

  return [...messageItems, ...turnItems].sort((a, b) => a.sortKey - b.sortKey);
}
