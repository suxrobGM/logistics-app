import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";
import { AgentStreamTailKey } from "@/shared/utils";
import { buildCopilotStream } from "./copilot-stream.utils";

/**
 * What breaks silently without these: a decision card drifting above the question that produced it,
 * or below the reply that summarizes it. Both look like the agent answered out of order, and
 * neither throws.
 */
function message(overrides: Partial<AgentMessageDto> = {}): AgentMessageDto {
  return { id: "m1", role: "user", sequence: 0, ...overrides };
}

function decision(overrides: Partial<AgentDecisionDto> = {}): AgentDecisionDto {
  return { id: "d1", ...overrides };
}

describe("buildCopilotStream", () => {
  it("sorts a session's decisions at firstAssistantSeq - 0.5, between the question and the reply", () => {
    const question = message({ id: "m-ask", role: "user", sequence: 9, sessionId: "s1" });
    const reply = message({ id: "m-reply", role: "assistant", sequence: 10, sessionId: "s1" });
    const d1 = decision({ id: "d1", sessionId: "s1" });

    const stream = buildCopilotStream([question, reply], [d1]);

    expect(stream.map((i) => (i.kind === "message" ? i.message.id : i.decision.id))).toEqual([
      "m-ask",
      "d1",
      "m-reply",
    ]);
    expect(stream[1].sortKey).toBe(9.5);
  });

  it("anchors on the assistant message, not the user message that shares the session", () => {
    const question = message({ id: "m-ask", role: "user", sequence: 1, sessionId: "s1" });
    const reply = message({ id: "m-reply", role: "assistant", sequence: 5, sessionId: "s1" });

    const stream = buildCopilotStream([question, reply], [decision({ sessionId: "s1" })]);

    expect(stream[1].sortKey).toBe(4.5);
  });

  it("parks a running turn's decisions at the tail, where the working indicator sits", () => {
    const earlier = message({ id: "m1", role: "assistant", sequence: 3, sessionId: "s-old" });
    const pending = decision({ id: "d-live", sessionId: "s-live" });

    const stream = buildCopilotStream([earlier], [pending]);

    expect(stream.at(-1)).toMatchObject({ kind: "decision", sortKey: AgentStreamTailKey });
  });

  it("keeps createdAt order within one session, since their sort keys tie", () => {
    const reply = message({ id: "m-reply", role: "assistant", sequence: 10, sessionId: "s1" });
    const second = decision({ id: "d2", sessionId: "s1", createdAt: "2026-01-01T00:00:02Z" });
    const first = decision({ id: "d1", sessionId: "s1", createdAt: "2026-01-01T00:00:01Z" });

    const stream = buildCopilotStream([reply], [second, first]);

    expect(stream.filter((i) => i.kind === "decision").map((i) => i.decision.id)).toEqual([
      "d1",
      "d2",
    ]);
  });

  it("returns an empty stream when there is nothing to show", () => {
    expect(buildCopilotStream([], [])).toEqual([]);
  });
});
