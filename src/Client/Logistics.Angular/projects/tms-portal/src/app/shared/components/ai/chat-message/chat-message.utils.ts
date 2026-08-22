import type { AgentMessageDto } from "@logistics/shared/api";

/**
 * Who a transcript row is attributable to, for a dispatch conversation the whole tenant reads.
 * Rows with no sender - the agent's own, and the broker-reply envelope - stay unlabelled.
 * A system note always names the person, never "You": it reports what they did.
 */
export function senderLabel(message: AgentMessageDto, currentUserId: string | null): string | null {
  if (!message.sentByUserId) return null;

  if (message.role === "system") return message.sentByName ?? null;

  return message.sentByUserId === currentUserId ? "You" : (message.sentByName ?? null);
}
