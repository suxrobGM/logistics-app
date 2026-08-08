import { DatePipe } from "@angular/common";
import { Component, computed, inject, input, output, signal } from "@angular/core";
import type { AgentConversationDto } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import {
  EmptyState,
  Icon,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiDialog,
  UiMenu,
  UiTextField,
  type UiMenuItem,
} from "@logistics/shared/ui";

/**
 * Persistent left sidebar of dispatch conversations - tenant-shared, so every dispatcher sees the
 * same list. Modeled on the copilot drawer's history view, but always visible rather than a
 * switched-to mode.
 */
@Component({
  selector: "app-dispatch-sidebar",
  templateUrl: "./dispatch-sidebar.html",
  imports: [
    DatePipe,
    EmptyState,
    Icon,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiDialog,
    UiMenu,
    UiTextField,
  ],
})
export class DispatchSidebar {
  private readonly toast = inject(ToastService);

  public readonly conversations = input.required<AgentConversationDto[]>();
  public readonly currentConversationId = input<string | null>(null);
  public readonly hasMore = input(false);
  public readonly loading = input(false);

  public readonly conversationSelected = output<string>();
  public readonly newChat = output<void>();
  public readonly rename = output<{ id: string; title: string }>();
  public readonly delete = output<string>();
  public readonly loadMore = output<void>();

  protected readonly renameTarget = signal<AgentConversationDto | null>(null);
  protected readonly renameTitle = signal("");

  /** Set by the row kebab before it opens the one shared menu - see `UiMenu`'s trigger contract. */
  protected readonly menuTarget = signal<AgentConversationDto | null>(null);

  protected readonly menuItems = computed<UiMenuItem[]>(() => {
    const conversation = this.menuTarget();
    if (!conversation) return [];

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
  });

  protected openRenameDialog(conversation: AgentConversationDto): void {
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
