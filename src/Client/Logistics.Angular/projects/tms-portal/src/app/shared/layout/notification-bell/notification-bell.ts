import { CommonModule } from "@angular/common";
import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
  viewChild,
  type OnInit,
} from "@angular/core";
import { RouterLink } from "@angular/router";
import type { NotificationDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import { Divider, Icon, OverlayBadge, Spinner, UiPopover, UiTooltip } from "@logistics/shared/ui";
import { NotificationService } from "@/core/services";

@Component({
  selector: "app-notification-bell",
  templateUrl: "./notification-bell.html",
  styleUrl: "./notification-bell.css",
  imports: [
    CommonModule,
    Divider,
    Icon,
    OverlayBadge,
    UiPopover,
    RelativeTimePipe,
    RouterLink,
    Spinner,
    UiTooltip,
  ],
})
export class NotificationBell implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly popover = viewChild<UiPopover>("popover");

  protected readonly notifications = this.notificationService.notifications;
  protected readonly unreadCount = this.notificationService.unreadCount;
  protected readonly isLoading = signal(false);
  protected readonly selectedNotification = signal<NotificationDto | null>(null);

  protected readonly displayedNotifications = computed(() => this.notifications().slice(0, 8));

  ngOnInit(): void {
    this.fetchNotifications();
    void this.notificationService.connect(this.destroyRef);
  }

  protected togglePopover(event: Event): void {
    this.popover()?.toggle(event);
  }

  protected onNotificationClick(notification: NotificationDto): void {
    if (this.selectedNotification()?.id === notification.id) {
      this.selectedNotification.set(null);
    } else {
      this.selectedNotification.set(notification);
      if (!notification.isRead) {
        this.markAsRead(notification);
      }
    }
  }

  protected markAllAsRead(): void {
    const unreadIds = this.notifications()
      .filter((n) => !n.isRead && n.id)
      .map((n) => n.id!);
    void this.notificationService.markAsRead(...unreadIds);
  }

  private async fetchNotifications(): Promise<void> {
    this.isLoading.set(true);
    try {
      await this.notificationService.load();
    } finally {
      this.isLoading.set(false);
    }
  }

  private markAsRead(notification: NotificationDto): void {
    if (notification.id) {
      void this.notificationService.markAsRead(notification.id);
    }
  }
}
