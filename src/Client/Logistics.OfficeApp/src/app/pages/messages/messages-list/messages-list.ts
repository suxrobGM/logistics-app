import { DatePipe } from "@angular/common";
import { Component, type OnDestroy, type OnInit, inject } from "@angular/core";
import { Router } from "@angular/router";
import { AvatarModule } from "primeng/avatar";
import { BadgeModule } from "primeng/badge";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import type { ConversationDto } from "@/core/api";
import { MessagingService } from "@/core/services";
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
  ],
})
export class MessagesListComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly messagingService = inject(MessagingService);
  protected readonly store = inject(MessagesStore);

  async ngOnInit(): Promise<void> {
    await this.messagingService.connect();
    await this.store.loadConversations();
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

  protected getParticipantNames(conversation: ConversationDto): string {
    return (conversation.participants ?? []).map((p) => p.employeeName || "Unknown").join(", ");
  }

  protected getInitials(conversation: ConversationDto): string {
    if (conversation.name) {
      return conversation.name
        .split(" ")
        .map((w) => w[0])
        .join("")
        .substring(0, 2)
        .toUpperCase();
    }

    const firstParticipant = (conversation.participants ?? [])[0];
    if (firstParticipant?.employeeName) {
      return firstParticipant.employeeName
        .split(" ")
        .map((w) => w[0])
        .join("")
        .substring(0, 2)
        .toUpperCase();
    }

    return "??";
  }
}
