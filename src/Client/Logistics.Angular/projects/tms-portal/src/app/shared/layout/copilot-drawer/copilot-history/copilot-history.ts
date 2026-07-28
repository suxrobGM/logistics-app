import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import type { AICopilotConversationDto } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { EmptyState, Icon, UiButton, UiMenu, type UiMenuItem } from "@logistics/shared/ui";

/** Conversation list shown by the drawer's history view. */
@Component({
  selector: "app-copilot-history",
  templateUrl: "./copilot-history.html",
  imports: [DatePipe, EmptyState, Icon, UiButton, UiMenu],
})
export class CopilotHistory {
  private readonly toast = inject(ToastService);

  public readonly conversations = input.required<AICopilotConversationDto[]>();
  public readonly open = output<string>();
  public readonly newChat = output<void>();
  public readonly delete = output<string>();

  protected menuItems(conversation: AICopilotConversationDto): UiMenuItem[] {
    return [
      {
        label: "Delete",
        icon: "trash",
        variant: "destructive",
        command: () =>
          this.toast.confirmDelete("conversation", () => this.delete.emit(conversation.id!)),
      },
    ];
  }
}
