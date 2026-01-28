import { DatePipe } from "@angular/common";
import { Component, type OnDestroy, type OnInit, inject } from "@angular/core";
import { Router } from "@angular/router";
import type { ConversationDto } from "@logistics/shared/api";
import { AvatarModule } from "primeng/avatar";
import { BadgeModule } from "primeng/badge";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { AuthService } from "@/core/auth";
import { ChatService } from "@/core/services";
import { UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";
import { MessagesStore } from "../store/messages.store";

@Component({
  selector: "app-messages-list",
  templateUrl: "./messages-list.html",
  providers: [MessagesStore],
  imports: [
    CardModule,
    ButtonModule,
    AvatarModule,
    BadgeModule,
    DividerModule,
    SkeletonModule,
    DatePipe,
    UserAvatar,
  ],
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
      const tenantChat = await this.store.getTenantChat();
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
