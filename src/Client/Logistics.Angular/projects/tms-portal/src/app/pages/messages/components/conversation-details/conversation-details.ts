import { DatePipe } from "@angular/common";
import { ChangeDetectionStrategy, Component, input, model } from "@angular/core";
import type { ConversationDto } from "@logistics/shared/api";
import { AvatarModule } from "primeng/avatar";
import { DrawerModule } from "primeng/drawer";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-conversation-details",
  templateUrl: "./conversation-details.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AvatarModule, DatePipe, DrawerModule],
})
export class ConversationDetails {
  readonly conversation = input<ConversationDto | null>(null);
  readonly currentUserId = input<string | null>(null);
  readonly visible = model(false);

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }

  protected getConversationTitle(conversation: ConversationDto | null): string {
    if (!conversation) return "";
    if (conversation.name) return conversation.name;
    return (conversation.participants ?? []).map((p) => p.employeeName || "Unknown").join(", ");
  }
}
