import type { AgentDecisionDto, AgentMessageDto, AgentSessionDto } from "@logistics/shared/api";

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
}

export type TranscriptItem = TranscriptTurn | TranscriptMessage;

const messageSortKey = (message: AgentMessageDto): number =>
  message.sequence ?? Number.MAX_SAFE_INTEGER;

/**
 * Merges messages and per-turn tool-activity timelines into one chronological stream:
 * - Messages sort by `sequence` (falls back to the end for an optimistic send with none yet).
 * - A turn (one session's decisions) sorts just before that session's first message, so the tool
 *   activity reads before the assistant's reply summarizing it. A turn with no message yet (still
 *   running) sorts at the very end, right where the "working..." indicator belongs.
 * - A session with no decisions renders no timeline - nothing to show.
 */
export function buildTranscriptStream(
  messages: readonly AgentMessageDto[],
  decisions: readonly AgentDecisionDto[],
  sessions: readonly AgentSessionDto[],
): TranscriptItem[] {
  const messageItems: TranscriptMessage[] = [];
  const firstSeqBySession = new Map<string, number>();

  for (const message of messages) {
    const sortKey = messageSortKey(message);
    messageItems.push({ kind: "message", message, sortKey });
    if (message.sessionId) {
      firstSeqBySession.set(
        message.sessionId,
        Math.min(firstSeqBySession.get(message.sessionId) ?? Number.MAX_SAFE_INTEGER, sortKey),
      );
    }
  }

  const decisionsBySession = new Map<string, AgentDecisionDto[]>();
  for (const decision of decisions) {
    if (!decision.sessionId) continue;
    const bucket = decisionsBySession.get(decision.sessionId);
    if (bucket) {
      bucket.push(decision);
    } else {
      decisionsBySession.set(decision.sessionId, [decision]);
    }
  }
  for (const bucket of decisionsBySession.values()) {
    bucket.sort((a, b) => (a.createdAt ?? "").localeCompare(b.createdAt ?? ""));
  }

  const turnItems: TranscriptTurn[] = [];
  for (const session of sessions) {
    const sessionDecisions = session.id ? decisionsBySession.get(session.id) : undefined;
    if (!sessionDecisions?.length) continue;

    const firstMessageSeq = firstSeqBySession.get(session.id!) ?? Number.MAX_SAFE_INTEGER;
    turnItems.push({
      kind: "turn",
      session,
      decisions: sessionDecisions,
      sortKey:
        firstMessageSeq === Number.MAX_SAFE_INTEGER
          ? Number.MAX_SAFE_INTEGER
          : firstMessageSeq - 0.5,
    });
  }

  return [...messageItems, ...turnItems].sort((a, b) => a.sortKey - b.sortKey);
}
