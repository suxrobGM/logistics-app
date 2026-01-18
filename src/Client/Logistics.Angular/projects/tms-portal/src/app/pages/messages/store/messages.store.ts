import { computed, inject } from "@angular/core";
import type { ConversationDto, MessageDto } from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { MessagingService } from "@/core/services";

interface MessagesState {
  conversations: ConversationDto[];
  currentConversation: ConversationDto | null;
  messages: MessageDto[];
  loading: boolean;
  error: string | null;
  hasMore: boolean;
  typingUsers: Set<string>;
  initialized: boolean;
}

const initialState: MessagesState = {
  conversations: [],
  currentConversation: null,
  messages: [],
  loading: false,
  error: null,
  hasMore: true,
  typingUsers: new Set(),
  initialized: false,
};

const MESSAGE_LOAD_BATCH_SIZE = 10;

export const MessagesStore = signalStore(
  withState(initialState),

  withComputed((store) => ({
    unreadCount: computed(() =>
      store.conversations().reduce((sum, c) => sum + (c.unreadCount ?? 0), 0),
    ),
  })),

  withMethods((store, messagingService = inject(MessagingService)) => {
    const setupRealtimeHandlers = () => {
      if (store.initialized()) return;

      messagingService.onReceiveMessage = (message: MessageDto) => {
        // Update current conversation messages
        if (store.currentConversation()?.id === message.conversationId) {
          patchState(store, { messages: [...store.messages(), message] });
        }

        // Update conversation list
        patchState(store, {
          conversations: store.conversations().map((c) => {
            if (c.id === message.conversationId) {
              return {
                ...c,
                lastMessage: message,
                lastMessageAt: message.sentAt,
                unreadCount: (c.unreadCount ?? 0) + 1,
              };
            }
            return c;
          }),
        });
      };

      messagingService.onMessageRead = (messageId) => {
        patchState(store, {
          messages: store.messages().map((m) => (m.id === messageId ? { ...m, isRead: true } : m)),
        });
      };

      messagingService.onTypingIndicator = (dto) => {
        if (dto.conversationId === store.currentConversation()?.id) {
          const newUsers = new Set(store.typingUsers());
          if (dto.isTyping) {
            newUsers.add(dto.userId);
          } else {
            newUsers.delete(dto.userId);
          }
          patchState(store, { typingUsers: newUsers });
        }
      };

      patchState(store, { initialized: true });
    };

    return {
      init(): void {
        setupRealtimeHandlers();
      },

      async loadConversations(participantId?: string): Promise<void> {
        setupRealtimeHandlers();
        patchState(store, { loading: true, error: null });

        try {
          const conversations = await messagingService.getConversations(participantId);
          patchState(store, { conversations, loading: false });
        } catch (e) {
          patchState(store, { error: "Failed to load conversations", loading: false });
          console.error(e);
        }
      },

      async selectConversation(conversation: ConversationDto): Promise<void> {
        if (!conversation.id) return;

        // Leave previous conversation
        const currentConvo = store.currentConversation();
        if (currentConvo?.id) {
          await messagingService.leaveConversation(currentConvo.id);
        }

        patchState(store, {
          currentConversation: conversation,
          messages: [],
          hasMore: true,
          typingUsers: new Set(),
        });

        // Join new conversation
        await messagingService.joinConversation(conversation.id);
        await this.loadMessages();
      },

      async loadMessages(append = false): Promise<void> {
        const conversation = store.currentConversation();
        if (!conversation?.id) return;

        patchState(store, { loading: true });

        try {
          const currentMessages = store.messages();
          const before =
            append && currentMessages.length > 0 ? currentMessages[0].sentAt : undefined;

          const newMessages = await messagingService.getMessages(
            conversation.id,
            MESSAGE_LOAD_BATCH_SIZE,
            0,
            before,
          );

          if (newMessages.length < MESSAGE_LOAD_BATCH_SIZE) {
            patchState(store, { hasMore: false });
          }

          if (append) {
            patchState(store, { messages: [...newMessages, ...currentMessages], loading: false });
          } else {
            patchState(store, { messages: newMessages, loading: false });
          }
        } catch (e) {
          patchState(store, { error: "Failed to load messages", loading: false });
          console.error(e);
        }
      },

      async sendMessage(content: string): Promise<void> {
        const conversation = store.currentConversation();
        if (!conversation?.id || !content.trim()) return;

        try {
          await messagingService.sendMessage({
            conversationId: conversation.id,
            content: content.trim(),
          });
          // Message will be added via SignalR callback
        } catch (e) {
          patchState(store, { error: "Failed to send message" });
          console.error(e);
        }
      },

      async markAsRead(messageId: string): Promise<void> {
        try {
          await messagingService.markAsRead(messageId);

          // Update unread count in conversation list
          patchState(store, {
            conversations: store.conversations().map((c) => {
              const unreadCount = c.unreadCount ?? 0;
              if (c.id === store.currentConversation()?.id && unreadCount > 0) {
                return { ...c, unreadCount: unreadCount - 1 };
              }
              return c;
            }),
          });
        } catch (e) {
          console.error("Failed to mark message as read", e);
        }
      },

      async sendTypingIndicator(isTyping: boolean): Promise<void> {
        const conversation = store.currentConversation();
        if (!conversation?.id) return;

        try {
          await messagingService.sendTypingIndicator(conversation.id, isTyping);
        } catch (e) {
          console.error("Failed to send typing indicator", e);
        }
      },

      async createConversation(
        participantIds: string[],
        name?: string,
        loadId?: string,
      ): Promise<ConversationDto> {
        const conversation = await messagingService.createConversation({
          participantIds,
          name,
          loadId,
        });

        patchState(store, { conversations: [conversation, ...store.conversations()] });
        return conversation;
      },

      async getTenantChat(): Promise<ConversationDto> {
        patchState(store, { loading: true });
        try {
          const tenantChat = await messagingService.getTenantChat();

          // Add to conversations if not already present
          const exists = store.conversations().some((c) => c.id === tenantChat.id);
          if (!exists) {
            patchState(store, { conversations: [tenantChat, ...store.conversations()] });
          }

          patchState(store, { loading: false });
          return tenantChat;
        } catch (e) {
          patchState(store, { error: "Failed to load team chat", loading: false });
          console.error(e);
          throw e;
        }
      },

      async cleanup(): Promise<void> {
        const currentConvo = store.currentConversation();
        if (currentConvo?.id) {
          await messagingService.leaveConversation(currentConvo.id);
        }
        patchState(store, { currentConversation: null, messages: [] });
      },

      reset(): void {
        patchState(store, initialState);
      },
    };
  }),
);
