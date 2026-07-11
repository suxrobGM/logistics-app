import { DatePipe } from "@angular/common";
import { Component, inject, type OnDestroy, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import type { ConversationDto } from "@logistics/shared/api";
import {
  Card,
  CountBadge,
  Icon,
  Skeleton,
  Stack,
  Typography,
  UiButton,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { ChatService } from "@/core/services";
import { UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";
import { MessagesStore } from "../store/messages.store";

@Component({
  selector: "app-messages-list",
  templateUrl: "./messages-list.html",
  providers: [MessagesStore],
  imports: [Card, CountBadge, DatePipe, Icon, Skeleton, Stack, Typography, UiButton, UserAvatar],
})
export class MessagesListComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly messagingService = inject(ChatService);
  protected readonly store = inject(MessagesStore);

  async ngOnInit(): Promise<void> {
    await this.messagingService.connect();
    const currentUserId = this.authService.getUserData()?.id;
    await this.store.loadConversations(currentUserId);
    await this.messagingService.getUnreadCount();
  }

  async ngOnDestroy(): Promise<void> {
    await this.store.cleanup();
  }

  protected selectConversation(conversation: ConversationDto): void {
    this.router.navigate(["/messages", conversation.id]);
  }

  protected createNewConversation(): void {
    this.router.navigate(["/messages", "new"]);
  }

  protected async openTeamChat(): Promise<void> {
    try {
      const tenantChat = await this.store.openTenantChat();
      this.router.navigate(["/messages", tenantChat.id]);
    } catch (e) {
      console.error("Failed to open team chat", e);
    }
  }

  protected getParticipantNames(conversation: ConversationDto): string {
    return (conversation.participants ?? []).map((p) => p.employeeName || "Unknown").join(", ");
  }

  protected getInitials(conversation: ConversationDto): string {
    if (conversation.name) {
      return Converters.getInitials(conversation.name);
    }
    const firstParticipant = (conversation.participants ?? [])[0];
    return Converters.getInitials(firstParticipant?.employeeName);
  }
}
