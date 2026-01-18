import { DatePipe } from "@angular/common";
import { ChangeDetectionStrategy, Component, input, output } from "@angular/core";
import type { MessageDto } from "@logistics/shared/api";
import { AvatarModule } from "primeng/avatar";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-message-bubble",
  templateUrl: "./message-bubble.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AvatarModule, DatePipe],
})
export class MessageBubble {
  readonly message = input.required<MessageDto>();
  readonly isOwn = input(false);
  readonly mouseEnter = output<void>();

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
