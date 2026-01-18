import { ChangeDetectionStrategy, Component, input, output } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { ConversationDto } from "@logistics/shared/api";
import { AvatarModule } from "primeng/avatar";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-conversation-header",
  templateUrl: "./conversation-header.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AvatarModule, ButtonModule, RouterLink, TooltipModule],
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
