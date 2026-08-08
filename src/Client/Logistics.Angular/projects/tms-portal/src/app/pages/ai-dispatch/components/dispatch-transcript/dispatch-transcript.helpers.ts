import type { AgentDecisionDto, AgentMessageDto, AgentSessionDto } from "@logistics/shared/api";

/** One agent turn's tool activity, grouped by session and rendered as a `ui-timeline`. */
export interface TranscriptTurn {
  readonly kind: "turn";
  readonly sessionId: string;
  readonly session: AgentSessionDto | undefined;
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
  const messageItems: TranscriptMessage[] = messages.map((message) => ({
    kind: "message",
    message,
    sortKey: messageSortKey(message),
  }));

  const turnItems: TranscriptTurn[] = sessions
    .map((session): TranscriptTurn | null => {
      const sessionDecisions = decisions
        .filter((d) => d.sessionId === session.id)
        .sort((a, b) => (a.createdAt ?? "").localeCompare(b.createdAt ?? ""));
      if (sessionDecisions.length === 0) return null;

      const firstMessageSeq = messages
        .filter((m) => m.sessionId === session.id)
        .map(messageSortKey)
        .reduce((min, seq) => Math.min(min, seq), Number.MAX_SAFE_INTEGER);

      return {
        kind: "turn",
        sessionId: session.id!,
        session,
        decisions: sessionDecisions,
        sortKey:
          firstMessageSeq === Number.MAX_SAFE_INTEGER
            ? Number.MAX_SAFE_INTEGER
            : firstMessageSeq - 0.5,
      };
    })
    .filter((turn): turn is TranscriptTurn => turn !== null);

  return [...messageItems, ...turnItems].sort((a, b) => a.sortKey - b.sortKey);
}
