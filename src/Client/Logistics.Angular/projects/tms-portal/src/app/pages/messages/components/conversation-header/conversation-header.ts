import { Component, input, output } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { ConversationDto } from "@logistics/shared/api";
import { Icon, Stack, Typography, UiButton } from "@logistics/shared/ui";
import { AvatarModule } from "primeng/avatar";
import { TooltipModule } from "primeng/tooltip";
import { UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-conversation-header",
  templateUrl: "./conversation-header.html",
  imports: [AvatarModule, Icon, RouterLink, Stack, TooltipModule, Typography, UiButton, UserAvatar],
})
export class ConversationHeader {
  readonly conversation = input<ConversationDto | null>(null);
  readonly isNewConversation = input(false);
  readonly isTyping = input(false);
  readonly toggleDetails = output<void>();

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }

  protected getConversationTitle(conversation: ConversationDto | null): string {
    if (!conversation) return "";
    if (conversation.name) return conversation.name;
    return (conversation.participants ?? []).map((p) => p.employeeName || "Unknown").join(", ");
  }
}
