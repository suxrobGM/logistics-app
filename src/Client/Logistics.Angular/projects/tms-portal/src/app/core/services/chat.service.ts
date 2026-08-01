import { inject, Injectable, signal } from "@angular/core";
import {
  Api,
  createConversation,
  getConversations,
  getMessages,
  getUnreadCount,
  markMessageRead,
  openTenantChat,
  sendMessage,
  type ConversationDto,
  type CreateConversationRequest,
  type MessageDto,
  type SendMessageRequest,
} from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

// SignalR-specific DTO (not generated from OpenAPI)
export interface TypingIndicatorDto {
  conversationId: string;
  userId: string;
  isTyping: boolean;
}

export interface MessageReadEvent {
  messageId: string;
  readBy: string;
}

/** Only one conversation is open at a time, so joining a new one supersedes the previous join. */
const ConversationGroup = "conversation";

@Injectable({ providedIn: "root" })
export class ChatService extends BaseHubConnection {
  private readonly api = inject(Api);

  public readonly unreadCount = signal(0);
  public readonly typingUsers = signal<Map<string, Set<string>>>(new Map());

  readonly messageReceived$ = this.event<MessageDto>("ReceiveMessage");
  readonly messageRead$ = this.mappedEvent(
    "MessageRead",
    (messageId: string, readBy: string): MessageReadEvent => ({ messageId, readBy }),
  );
  readonly typingIndicator$ = this.mappedEvent(
    "TypingIndicator",
    (conversationId: string, userId: string, isTyping: boolean): TypingIndicatorDto => ({
      conversationId,
      userId,
      isTyping,
    }),
  );

  constructor() {
    super("chat");
  }

  joinConversation(conversationId: string): Promise<void> {
    return this.joinGroup(ConversationGroup, "JoinConversation", conversationId);
  }

  leaveConversation(conversationId: string): Promise<void> {
    return this.leaveGroup(ConversationGroup, "LeaveConversation", conversationId);
  }

  async sendTypingIndicator(conversationId: string, isTyping: boolean): Promise<void> {
    await this.hubConnection.invoke("SendTypingIndicator", conversationId, isTyping);
  }

  getConversations(participantId?: string, loadId?: string): Promise<ConversationDto[]> {
    return this.api.invoke(getConversations, { ParticipantId: participantId, LoadId: loadId });
  }

  createConversation(request: CreateConversationRequest): Promise<ConversationDto> {
    return this.api.invoke(createConversation, { body: request });
  }

  getMessages(
    conversationId: string,
    limit = 50,
    offset = 0,
    before?: string,
  ): Promise<MessageDto[]> {
    return this.api.invoke(getMessages, { conversationId, limit, offset, before });
  }

  sendMessage(request: SendMessageRequest): Promise<MessageDto> {
    return this.api.invoke(sendMessage, { body: request });
  }

  markAsRead(messageId: string): Promise<void> {
    return this.api.invoke(markMessageRead, { messageId });
  }

  async getUnreadCount(): Promise<number> {
    const count = await this.api.invoke(getUnreadCount);
    this.unreadCount.set(count);
    return count;
  }

  openTenantChat(): Promise<ConversationDto> {
    return this.api.invoke(openTenantChat);
  }
}
