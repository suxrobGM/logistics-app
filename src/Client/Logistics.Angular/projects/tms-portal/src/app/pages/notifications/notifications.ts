import { DatePipe } from "@angular/common";
import { Component, type OnDestroy, type OnInit, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { SearchInput } from "@logistics/shared";
import {
  Api,
  type NotificationDto,
  getNotifications,
  updateNotification,
} from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectButtonModule } from "primeng/selectbutton";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
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
    CardModule,
    ButtonModule,
    DividerModule,
    ProgressSpinnerModule,
    TagModule,
    RelativeTimePipe,
    DatePipe,
    SelectButtonModule,
    CheckboxModule,
    FormsModule,
    TooltipModule,
    SearchInput,
  ],
})
export class NotificationsComponent implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly notificationService = inject(NotificationService);
  private readonly toastService = inject(ToastService);

  protected readonly notifications = signal<NotificationDto[]>([]);
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

  protected readonly unreadCount = computed(
    () => this.notifications().filter((n) => !n.isRead).length,
  );

  protected readonly hasSelection = computed(() => this.selectedIds().size > 0);

  protected readonly allSelected = computed(() => {
    const filtered = this.filteredNotifications();
    if (filtered.length === 0) return false;
    return filtered.every((n) => n.id && this.selectedIds().has(n.id));
  });

  async ngOnInit(): Promise<void> {
    await this.fetchNotifications();
    this.setupRealTimeNotifications();
  }

  ngOnDestroy(): void {
    this.notificationService.disconnect();
  }

  protected async fetchNotifications(): Promise<void> {
    this.isLoading.set(true);

    // Fetch last 30 days of notifications
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);

    const result = await this.api.invoke(getNotifications, {
      StartDate: startDate.toISOString(),
      EndDate: endDate.toISOString(),
    });

    if (result) {
      this.notifications.set(result);
    }

    this.isLoading.set(false);
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

    await this.api.invoke(updateNotification, {
      id: notification.id,
      body: { isRead: true },
    });

    this.notifications.update((list) =>
      list.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)),
    );
  }

  protected async markSelectedAsRead(): Promise<void> {
    const ids = Array.from(this.selectedIds());
    if (ids.length === 0) return;

    for (const id of ids) {
      await this.api.invoke(updateNotification, {
        id,
        body: { isRead: true },
      });
    }

    this.notifications.update((list) =>
      list.map((n) => (n.id && ids.includes(n.id) ? { ...n, isRead: true } : n)),
    );

    this.selectedIds.set(new Set());
    this.toastService.showSuccess(`Marked ${ids.length} notification(s) as read`);
  }

  protected async markAllAsRead(): Promise<void> {
    const unread = this.notifications().filter((n) => !n.isRead && n.id);
    if (unread.length === 0) return;

    for (const notification of unread) {
      if (notification.id) {
        await this.api.invoke(updateNotification, {
          id: notification.id,
          body: { isRead: true },
        });
      }
    }

    this.notifications.update((list) => list.map((n) => ({ ...n, isRead: true })));
    this.toastService.showSuccess(`Marked ${unread.length} notification(s) as read`);
  }

  private setupRealTimeNotifications(): void {
    this.notificationService.connect();
    this.notificationService.onReceiveNotification = (notification) => {
      this.toastService.showSuccess(notification.message ?? "", notification.title ?? undefined);
      this.notifications.update((current) => [notification, ...current]);
    };
  }
}
