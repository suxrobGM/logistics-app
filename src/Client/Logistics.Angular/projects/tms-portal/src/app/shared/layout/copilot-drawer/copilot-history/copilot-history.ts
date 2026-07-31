import { DatePipe } from "@angular/common";
import { Component, inject, input, output, signal } from "@angular/core";
import type { AICopilotConversationDto } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import {
  EmptyState,
  Icon,
  UiButton,
  UiDialog,
  UiMenu,
  UiTextField,
  type UiMenuItem,
} from "@logistics/shared/ui";

/** Conversation list shown by the drawer's history view. */
@Component({
  selector: "app-copilot-history",
  templateUrl: "./copilot-history.html",
  imports: [DatePipe, EmptyState, Icon, UiButton, UiDialog, UiMenu, UiTextField],
})
export class CopilotHistory {
  private readonly toast = inject(ToastService);

  public readonly conversations = input.required<AICopilotConversationDto[]>();
  public readonly hasMore = input(false);
  public readonly open = output<string>();
  public readonly newChat = output<void>();
  public readonly delete = output<string>();
  public readonly loadMore = output<void>();
  public readonly rename = output<{ id: string; title: string }>();

  protected readonly renameTarget = signal<AICopilotConversationDto | null>(null);
  protected readonly renameTitle = signal("");

  protected menuItems(conversation: AICopilotConversationDto): UiMenuItem[] {
    return [
      {
        label: "Rename",
        icon: "pencil",
        command: () => this.openRenameDialog(conversation),
      },
      {
        label: "Delete",
        icon: "trash",
        variant: "destructive",
        command: () =>
          this.toast.confirmDelete("conversation", () => this.delete.emit(conversation.id!)),
      },
    ];
  }

  protected openRenameDialog(conversation: AICopilotConversationDto): void {
    this.renameTarget.set(conversation);
    this.renameTitle.set(conversation.title ?? "");
  }

  protected confirmRename(): void {
    const target = this.renameTarget();
    const title = this.renameTitle().trim();
    if (!target?.id || !title) return;
    this.rename.emit({ id: target.id, title });
    this.closeRenameDialog();
  }

  protected closeRenameDialog(): void {
    this.renameTarget.set(null);
    this.renameTitle.set("");
  }
}
