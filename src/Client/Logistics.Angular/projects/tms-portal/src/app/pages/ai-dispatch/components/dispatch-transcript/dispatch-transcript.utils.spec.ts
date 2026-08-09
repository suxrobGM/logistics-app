import type { AgentDecisionDto, AgentMessageDto, AgentSessionDto } from "@logistics/shared/api";
import { AgentStreamTailKey } from "@/shared/utils";
import {
  buildTranscriptStream,
  type TranscriptMessage,
  type TranscriptTurn,
} from "./dispatch-transcript.utils";

function message(overrides: Partial<AgentMessageDto> = {}): AgentMessageDto {
  return { id: "m1", role: "user", sequence: 0, ...overrides };
}

function decision(overrides: Partial<AgentDecisionDto> = {}): AgentDecisionDto {
  return { id: "d1", ...overrides };
}

function session(overrides: Partial<AgentSessionDto> = {}): AgentSessionDto {
  return { id: "s1", ...overrides };
}

describe("buildTranscriptStream", () => {
  it("sorts a turn's tool activity at firstMessageSeq - 0.5, immediately before its own reply", () => {
    const reply = message({ id: "m-reply", role: "assistant", sequence: 10, sessionId: "s1" });
    const s1 = session({ id: "s1" });
    const d1 = decision({ id: "d1", sessionId: "s1" });

    const stream = buildTranscriptStream([reply], [d1], [s1]);

    expect(stream).toHaveLength(2);
    const [turn, msg] = stream as [TranscriptTurn, TranscriptMessage];
    expect(turn.kind).toBe("turn");
    expect(turn.sortKey).toBe(9.5);
    expect(msg.kind).toBe("message");
    expect(msg.sortKey).toBe(10);
  });

  it("a session with no decisions renders no timeline", () => {
    const reply = message({ id: "m-reply", role: "assistant", sequence: 10, sessionId: "s1" });
    const s1 = session({ id: "s1" });

    const stream = buildTranscriptStream([reply], [], [s1]);

    expect(stream).toEqual([expect.objectContaining({ kind: "message" })]);
  });

  it("a running turn (no message yet references the session) sorts last", () => {
    const earlyReply = message({ id: "m-early", role: "assistant", sequence: 1 });
    const runningSession = session({ id: "s-running" });
    const runningDecision = decision({ id: "d-running", sessionId: "s-running" });

    const stream = buildTranscriptStream([earlyReply], [runningDecision], [runningSession]);

    expect(stream).toHaveLength(2);
    const runningTurn = stream.find((item) => item.kind === "turn") as TranscriptTurn;
    expect(runningTurn.sortKey).toBe(AgentStreamTailKey);
    expect(stream.at(-1)).toBe(runningTurn);
  });

  it("marks the LAST assistant message of a session as the report and attaches the session", () => {
    const first = message({ id: "m1", role: "assistant", sequence: 10, sessionId: "s1" });
    const second = message({ id: "m2", role: "assistant", sequence: 20, sessionId: "s1" });
    const s1 = session({ id: "s1", totalTokensUsed: 500 });

    const stream = buildTranscriptStream([first, second], [], [s1]);
    const messages = stream.filter((item) => item.kind === "message") as TranscriptMessage[];
    const firstOut = messages.find((m) => m.message.id === "m1")!;
    const secondOut = messages.find((m) => m.message.id === "m2")!;

    expect(firstOut.isReport).toBe(false);
    expect(firstOut.session).toBeUndefined();
    expect(secondOut.isReport).toBe(true);
    expect(secondOut.session).toBe(s1);
  });

  it("a non-assistant message in a session is never marked as the report", () => {
    const userMsg = message({ id: "m-user", role: "user", sequence: 5, sessionId: "s1" });
    const s1 = session({ id: "s1" });

    const stream = buildTranscriptStream([userMsg], [], [s1]);
    const [out] = stream as [TranscriptMessage];

    expect(out.isReport).toBe(false);
  });

  it("a message without a sessionId is never marked as the report, even as the assistant", () => {
    const orphanReply = message({
      id: "m-orphan",
      role: "assistant",
      sequence: 5,
      sessionId: undefined,
    });

    const stream = buildTranscriptStream([orphanReply], [], []);
    const [out] = stream as [TranscriptMessage];

    expect(out.isReport).toBe(false);
    expect(out.session).toBeUndefined();
  });

  it("still marks isReport when the session isn't in the sessions list, but leaves session undefined", () => {
    const orphanSessionReply = message({
      id: "m-orphan-session",
      role: "assistant",
      sequence: 5,
      sessionId: "s-missing",
    });

    const stream = buildTranscriptStream([orphanSessionReply], [], []);
    const [out] = stream as [TranscriptMessage];

    expect(out.isReport).toBe(true);
    expect(out.session).toBeUndefined();
  });
});
