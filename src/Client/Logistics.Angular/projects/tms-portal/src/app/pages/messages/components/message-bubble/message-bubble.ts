import { DatePipe } from "@angular/common";
import { Component, input, output } from "@angular/core";
import type { MessageDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";
import { UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-message-bubble",
  templateUrl: "./message-bubble.html",
  imports: [DatePipe, Icon, UserAvatar],
})
export class MessageBubble {
  readonly message = input.required<MessageDto>();
  readonly isOwn = input(false);
  readonly mouseEnter = output<void>();

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
