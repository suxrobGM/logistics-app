import { Injectable, inject, signal } from "@angular/core";
import {
  Api,
  type ConversationDto,
  type CreateConversationRequest,
  type MessageDto,
  type SendMessageRequest,
  createConversation,
  getConversations,
  getMessages,
  getTenantChat,
  getUnreadCount,
  markMessageRead,
  sendMessage,
} from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

// SignalR-specific DTO (not generated from OpenAPI)
export interface TypingIndicatorDto {
  conversationId: string;
  userId: string;
  isTyping: boolean;
}

@Injectable({ providedIn: "root" })
export class ChatService extends BaseHubConnection {
  private readonly api = inject(Api);

  public readonly unreadCount = signal(0);
  public readonly typingUsers = signal<Map<string, Set<string>>>(new Map());

  constructor() {
    super("chat");
  }

  set onReceiveMessage(callback: (message: MessageDto) => void) {
    this.hubConnection.on("ReceiveMessage", callback);
  }

  set onMessageRead(callback: (messageId: string, readBy: string) => void) {
    this.hubConnection.on("MessageRead", callback);
  }

  set onTypingIndicator(callback: (dto: TypingIndicatorDto) => void) {
    this.hubConnection.on(
      "TypingIndicator",
      (conversationId: string, userId: string, isTyping: boolean) => {
        callback({ conversationId, userId, isTyping });
      },
    );
  }

  async joinConversation(conversationId: string): Promise<void> {
    await this.hubConnection.invoke("JoinConversation", conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    await this.hubConnection.invoke("LeaveConversation", conversationId);
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

  getTenantChat(): Promise<ConversationDto> {
    return this.api.invoke(getTenantChat);
  }
}
