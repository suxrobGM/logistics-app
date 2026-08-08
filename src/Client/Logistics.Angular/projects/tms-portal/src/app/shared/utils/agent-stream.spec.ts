import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";
import {
  AgentStreamTailKey,
  firstMessageSequenceBySession,
  groupDecisionsBySession,
  messageSortKey,
  sessionSortKey,
} from "./agent-stream";

function message(overrides: Partial<AgentMessageDto> = {}): AgentMessageDto {
  return { id: "m1", role: "user", ...overrides };
}

function decision(overrides: Partial<AgentDecisionDto> = {}): AgentDecisionDto {
  return { id: "d1", ...overrides };
}

describe("messageSortKey", () => {
  it("uses the server-assigned sequence when present", () => {
    expect(messageSortKey(message({ sequence: 7 }))).toBe(7);
  });

  it("sorts a message with no sequence (an optimistic echo) to the tail", () => {
    expect(messageSortKey(message({ sequence: undefined }))).toBe(AgentStreamTailKey);
  });

  it("a sequence of 0 is not treated as missing", () => {
    expect(messageSortKey(message({ sequence: 0 }))).toBe(0);
  });
});

describe("firstMessageSequenceBySession", () => {
  it("takes the lowest sequence per session", () => {
    const messages = [
      message({ sessionId: "s1", sequence: 10 }),
      message({ sessionId: "s1", sequence: 3 }),
      message({ sessionId: "s1", sequence: 20 }),
    ];

    expect(firstMessageSequenceBySession(messages)).toEqual(new Map([["s1", 3]]));
  });

  it("keeps sessions independent", () => {
    const messages = [
      message({ sessionId: "s1", sequence: 10 }),
      message({ sessionId: "s2", sequence: 1 }),
    ];

    expect(firstMessageSequenceBySession(messages)).toEqual(
      new Map([
        ["s1", 10],
        ["s2", 1],
      ]),
    );
  });

  it("ignores messages with no sessionId", () => {
    const messages = [message({ sessionId: undefined, sequence: 1 })];

    expect(firstMessageSequenceBySession(messages)).toEqual(new Map());
  });

  it("a session whose only message has no sequence records the tail key", () => {
    const messages = [message({ sessionId: "s1", sequence: undefined })];

    expect(firstMessageSequenceBySession(messages)).toEqual(new Map([["s1", AgentStreamTailKey]]));
  });
});

describe("sessionSortKey", () => {
  it("sorts just before the session's first message", () => {
    expect(sessionSortKey(10)).toBe(9.5);
  });

  it("sorts a session with no message yet (the running turn) to the very end", () => {
    expect(sessionSortKey(undefined)).toBe(AgentStreamTailKey);
  });

  it("treats a first-message sequence pinned at the tail key the same as undefined", () => {
    expect(sessionSortKey(AgentStreamTailKey)).toBe(AgentStreamTailKey);
  });

  it("handles a first sequence of 0 (not confused with the falsy/undefined case)", () => {
    expect(sessionSortKey(0)).toBe(-0.5);
  });
});

describe("groupDecisionsBySession", () => {
  it("buckets decisions by sessionId, ascending by createdAt within each bucket", () => {
    const d1 = decision({ id: "d1", sessionId: "s1", createdAt: "2026-01-01T00:00:03Z" });
    const d2 = decision({ id: "d2", sessionId: "s1", createdAt: "2026-01-01T00:00:01Z" });
    const d3 = decision({ id: "d3", sessionId: "s2", createdAt: "2026-01-01T00:00:02Z" });

    const grouped = groupDecisionsBySession([d1, d2, d3]);

    expect([...grouped.keys()]).toEqual(expect.arrayContaining(["s1", "s2"]));
    expect(grouped.get("s1")).toEqual([d2, d1]);
    expect(grouped.get("s2")).toEqual([d3]);
  });

  it("groups decisions with no sessionId into a single ungrouped bucket", () => {
    const d1 = decision({ id: "d1", sessionId: undefined });
    const d2 = decision({ id: "d2", sessionId: undefined });

    const grouped = groupDecisionsBySession([d1, d2]);

    expect(grouped.get("")).toEqual([d1, d2]);
  });

  it("returns an empty map for no decisions", () => {
    expect(groupDecisionsBySession([])).toEqual(new Map());
  });
});
