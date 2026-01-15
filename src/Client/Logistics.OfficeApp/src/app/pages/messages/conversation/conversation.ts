import { DatePipe } from "@angular/common";
import {
  Component,
  ElementRef,
  type OnDestroy,
  type OnInit,
  inject,
  signal,
  viewChild,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { AvatarModule } from "primeng/avatar";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SkeletonModule } from "primeng/skeleton";
import type { ConversationDto, MessageDto } from "@/core/api";
import { AuthService } from "@/core/auth";
import { MessagingService } from "@/core/services";
import { MessagesStore } from "../store/messages.store";

@Component({
  selector: "app-conversation",
  templateUrl: "./conversation.html",
  providers: [MessagesStore],
  imports: [
    CardModule,
    ButtonModule,
    AvatarModule,
    InputTextModule,
    FormsModule,
    SkeletonModule,
    ProgressSpinnerModule,
    RouterLink,
    DatePipe,
  ],
})
export class ConversationComponent implements OnInit, OnDestroy {
  protected readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");
  protected readonly messageInput = viewChild<ElementRef<HTMLInputElement>>("messageInput");

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly messagingService = inject(MessagingService);
  private readonly authService = inject(AuthService);
  protected readonly store = inject(MessagesStore);

  protected messageContent = signal("");
  protected currentUserId = signal<string | null>(null);
  private typingTimeout: ReturnType<typeof setTimeout> | null = null;

  async ngOnInit(): Promise<void> {
    await this.messagingService.connect();

    const conversationId = this.route.snapshot.params["id"];
    if (!conversationId || conversationId === "new") {
      return;
    }

    // Get current user ID from auth service
    const userData = this.authService.getUserData();
    if (userData) {
      this.currentUserId.set(userData.id);
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

    await this.store.sendMessage(content);
    this.messageContent.set("");

    // Clear typing indicator
    await this.store.sendTypingIndicator(false);

    // Scroll to bottom after sending
    setTimeout(() => this.scrollToBottom(), 100);
  }

  protected onKeyDown(event: KeyboardEvent): void {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  protected onInput(): void {
    // Send typing indicator
    this.store.sendTypingIndicator(true);

    // Clear previous timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }

    // Stop typing indicator after 2 seconds of inactivity
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

  protected getInitials(name?: string | null): string {
    if (!name) return "??";
    return name
      .split(" ")
      .map((w) => w[0])
      .join("")
      .substring(0, 2)
      .toUpperCase();
  }

  protected getConversationTitle(conversation: ConversationDto | null): string {
    if (!conversation) return "";
    if (conversation.name) return conversation.name;
    return (conversation.participants ?? []).map((p) => p.employeeName || "Unknown").join(", ");
  }

  protected markMessageAsRead(message: MessageDto): void {
    if (!message.isRead && !this.isOwnMessage(message) && message.id) {
      this.store.markAsRead(message.id);
    }
  }

  private scrollToBottom(): void {
    const containerRef = this.messagesContainer();
    if (containerRef) {
      const container = containerRef.nativeElement;
      container.scrollTop = container.scrollHeight;
    }
  }
}
