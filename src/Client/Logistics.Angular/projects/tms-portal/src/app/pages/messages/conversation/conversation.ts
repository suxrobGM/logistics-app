import {
  Component,
  ElementRef,
  type OnDestroy,
  type OnInit,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import type { ConversationDto, EmployeeDto, MessageDto } from "@logistics/shared/api";
import { EmptyState } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { SkeletonModule } from "primeng/skeleton";
import { AuthService } from "@/core/auth";
import { ChatService } from "@/core/services";
import {
  ConversationDetails,
  ConversationHeader,
  MessageBubble,
  RecipientSelector,
} from "../components";
import { MessagesStore } from "../store/messages.store";

@Component({
  selector: "app-conversation",
  templateUrl: "./conversation.html",
  providers: [MessagesStore],
  imports: [
    ButtonModule,
    InputTextModule,
    FormsModule,
    SkeletonModule,
    EmptyState,
    ConversationHeader,
    ConversationDetails,
    MessageBubble,
    RecipientSelector,
  ],
})
export class ConversationComponent implements OnInit, OnDestroy {
  protected readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");

  private readonly router = inject(Router);
  private readonly messagingService = inject(ChatService);
  private readonly authService = inject(AuthService);
  protected readonly store = inject(MessagesStore);

  readonly id = input.required<string>();
  protected messageContent = signal("");
  protected currentUserId = signal<string | null>(null);
  protected showDetails = signal(false);
  protected isNewConversation = signal(false);
  protected selectedRecipient = signal<EmployeeDto | null>(null);
  protected isCreatingConversation = signal(false);
  private typingTimeout: ReturnType<typeof setTimeout> | null = null;
  private initialized = false;

  constructor() {
    effect(() => {
      const conversationId = this.id();
      if (this.initialized && conversationId) {
        this.handleConversationId(conversationId);
      }
    });
  }

  async ngOnInit(): Promise<void> {
    await this.messagingService.connect();

    const userData = this.authService.getUserData();
    if (userData) {
      this.currentUserId.set(userData.id);
    }

    this.initialized = true;
    const conversationId = this.id();
    if (conversationId) {
      await this.handleConversationId(conversationId);
    }
  }

  private async handleConversationId(conversationId: string): Promise<void> {
    if (!conversationId || conversationId === "new") {
      this.isNewConversation.set(true);
      return;
    }

    await this.store.loadConversations();
    const conversation = this.store.conversations().find((c) => c.id === conversationId);

    if (conversation) {
      await this.store.selectConversation(conversation);
      this.scrollToBottom();
    } else {
      this.router.navigate(["/messages"]);
    }
  }

  async ngOnDestroy(): Promise<void> {
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
    await this.store.cleanup();
  }

  protected async sendMessage(): Promise<void> {
    const content = this.messageContent();
    if (!content.trim()) return;

    if (this.isNewConversation()) {
      const recipient = this.selectedRecipient();
      const currentUser = this.currentUserId();
      if (!recipient?.id || !currentUser) return;

      this.isCreatingConversation.set(true);
      try {
        // Load conversations to check for existing one
        await this.store.loadConversations();
        const existingConversation = this.findExistingConversation(currentUser, recipient.id);

        if (existingConversation) {
          // Use existing conversation
          await this.store.selectConversation(existingConversation);
          this.isNewConversation.set(false);
          await this.store.sendMessage(content);
          this.messageContent.set("");
          this.router.navigate(["/messages", existingConversation.id], { replaceUrl: true });
        } else {
          // Create new conversation
          const conversation = await this.store.createConversation([currentUser, recipient.id]);
          await this.store.selectConversation(conversation);
          this.isNewConversation.set(false);
          await this.store.sendMessage(content);
          this.messageContent.set("");
          this.router.navigate(["/messages", conversation.id], { replaceUrl: true });
        }
      } finally {
        this.isCreatingConversation.set(false);
      }
      return;
    }

    await this.store.sendMessage(content);
    this.messageContent.set("");
    await this.store.sendTypingIndicator(false);

    setTimeout(() => this.scrollToBottom(), 100);
  }

  protected onRecipientChange(recipient: EmployeeDto | null): void {
    this.selectedRecipient.set(recipient);
  }

  protected canSendMessage(): boolean {
    const hasContent = !!this.messageContent().trim();
    if (this.isNewConversation()) {
      return hasContent && !!this.selectedRecipient();
    }
    return hasContent;
  }

  protected onKeyDown(event: KeyboardEvent): void {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  protected onInput(): void {
    this.store.sendTypingIndicator(true);

    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }

    this.typingTimeout = setTimeout(() => {
      this.store.sendTypingIndicator(false);
    }, 2000);
  }

  protected async loadMore(): Promise<void> {
    await this.store.loadMessages(true);
  }

  protected isOwnMessage(message: MessageDto): boolean {
    return message.senderId === this.currentUserId();
  }

  protected markMessageAsRead(message: MessageDto): void {
    if (!message.isRead && !this.isOwnMessage(message) && message.id) {
      this.store.markAsRead(message.id);
    }
  }

  protected toggleDetails(): void {
    this.showDetails.update((v) => !v);
  }

  private scrollToBottom(): void {
    const containerRef = this.messagesContainer();
    if (containerRef) {
      const container = containerRef.nativeElement;
      container.scrollTop = container.scrollHeight;
    }
  }

  private findExistingConversation(
    currentUserId: string,
    recipientId: string,
  ): ConversationDto | null {
    const targetIds = new Set([currentUserId, recipientId]);

    return (
      this.store.conversations().find((conv) => {
        // Skip team chats
        if (conv.isTenantChat) return false;

        const participantIds = new Set((conv.participants ?? []).map((p) => p.employeeId));

        // Check if participants match exactly (same size and same members)
        if (participantIds.size !== targetIds.size) return false;
        for (const id of targetIds) {
          if (!participantIds.has(id)) return false;
        }
        return true;
      }) ?? null
    );
  }
}
