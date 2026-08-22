import type { AgentMessageDto } from "@logistics/shared/api";
import { senderLabel } from "./chat-message.utils";

const me = "u-me";

function message(overrides: Partial<AgentMessageDto> = {}): AgentMessageDto {
  return { id: "m1", role: "user", sequence: 0, ...overrides };
}

describe("senderLabel", () => {
  it("labels my own message 'You'", () => {
    const label = senderLabel(message({ sentByUserId: me, sentByName: "Sarah Thompson" }), me);

    expect(label).toBe("You");
  });

  it("names another dispatcher", () => {
    const label = senderLabel(
      message({ sentByUserId: "u-other", sentByName: "Marcus Johnson" }),
      me,
    );

    expect(label).toBe("Marcus Johnson");
  });

  it("labels nothing when the row has no sender", () => {
    expect(senderLabel(message({ role: "assistant" }), me)).toBeNull();
  });

  it("labels nothing when the sender has no name left", () => {
    expect(senderLabel(message({ sentByUserId: "u-gone" }), me)).toBeNull();
  });

  it("names the actor on a system note, never 'You'", () => {
    const label = senderLabel(
      message({ role: "system", sentByUserId: me, sentByName: "Sarah Thompson" }),
      me,
    );

    expect(label).toBe("Sarah Thompson");
  });
});
