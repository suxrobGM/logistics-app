import { DatePipe } from "@angular/common";
import { Component, computed, DestroyRef, inject, signal, type OnInit } from "@angular/core";
import { SearchField } from "@logistics/shared";
import type { NotificationDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import {
  Badge,
  Card,
  Divider,
  Icon,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiCheckboxField,
  UiToggleGroup,
  UiTooltip,
} from "@logistics/shared/ui";
import { NotificationService, ToastService } from "@/core/services";

type FilterType = "all" | "unread" | "read";

interface FilterOption {
  label: string;
  value: FilterType;
}

@Component({
  selector: "app-notifications",
  templateUrl: "./notifications.html",
  styleUrl: "./notifications.css",
  imports: [
    Badge,
    Card,
    DatePipe,
    Divider,
    Icon,
    RelativeTimePipe,
    SearchField,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiCheckboxField,
    UiToggleGroup,
    UiTooltip,
  ],
})
export class NotificationsComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly notifications = this.notificationService.notifications;
  protected readonly unreadCount = this.notificationService.unreadCount;
  protected readonly isLoading = signal(false);
  protected readonly searchQuery = signal("");
  protected readonly filterType = signal<FilterType>("all");
  protected readonly selectedIds = signal<Set<string>>(new Set());

  protected readonly filterOptions: FilterOption[] = [
    { label: "All", value: "all" },
    { label: "Unread", value: "unread" },
    { label: "Read", value: "read" },
  ];

  protected readonly filteredNotifications = computed(() => {
    let result = this.notifications();

    // Apply filter
    const filter = this.filterType();
    if (filter === "unread") {
      result = result.filter((n) => !n.isRead);
    } else if (filter === "read") {
      result = result.filter((n) => n.isRead);
    }

    // Apply search
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(
        (n) => n.title?.toLowerCase().includes(query) || n.message?.toLowerCase().includes(query),
      );
    }

    return result;
  });

  protected readonly hasSelection = computed(() => this.selectedIds().size > 0);

  protected readonly allSelected = computed(() => {
    const filtered = this.filteredNotifications();
    if (filtered.length === 0) return false;
    return filtered.every((n) => n.id && this.selectedIds().has(n.id));
  });

  async ngOnInit(): Promise<void> {
    await this.fetchNotifications();
    void this.notificationService.connect(this.destroyRef);
  }

  protected async fetchNotifications(): Promise<void> {
    this.isLoading.set(true);
    try {
      await this.notificationService.load();
    } finally {
      this.isLoading.set(false);
    }
  }

  protected toggleSelection(notification: NotificationDto): void {
    if (!notification.id) return;

    this.selectedIds.update((ids) => {
      const newIds = new Set(ids);
      if (newIds.has(notification.id!)) {
        newIds.delete(notification.id!);
      } else {
        newIds.add(notification.id!);
      }
      return newIds;
    });
  }

  protected toggleSelectAll(): void {
    if (this.allSelected()) {
      this.selectedIds.set(new Set());
    } else {
      const ids = this.filteredNotifications()
        .map((n) => n.id)
        .filter((id): id is string => !!id);
      this.selectedIds.set(new Set(ids));
    }
  }

  protected isSelected(notification: NotificationDto): boolean {
    return !!notification.id && this.selectedIds().has(notification.id);
  }

  protected clearSelection(): void {
    this.selectedIds.set(new Set());
  }

  protected async markAsRead(notification: NotificationDto): Promise<void> {
    if (!notification.id || notification.isRead) return;

    await this.notificationService.markAsRead(notification.id);
  }

  protected async markSelectedAsRead(): Promise<void> {
    const ids = Array.from(this.selectedIds());
    if (ids.length === 0) return;

    await this.notificationService.markAsRead(...ids);
    this.selectedIds.set(new Set());
    this.toastService.showSuccess(`Marked ${ids.length} notification(s) as read`);
  }

  protected async markAllAsRead(): Promise<void> {
    const unreadIds = this.notifications()
      .filter((n) => !n.isRead && n.id)
      .map((n) => n.id!);
    if (unreadIds.length === 0) return;

    await this.notificationService.markAsRead(...unreadIds);
    this.toastService.showSuccess(`Marked ${unreadIds.length} notification(s) as read`);
  }
}
