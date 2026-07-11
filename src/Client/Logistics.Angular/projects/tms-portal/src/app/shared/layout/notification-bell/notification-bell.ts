import { CommonModule } from "@angular/common";
import {
  Component,
  computed,
  inject,
  input,
  signal,
  viewChild,
  type OnDestroy,
  type OnInit,
} from "@angular/core";
import { RouterLink } from "@angular/router";
import type { NotificationDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import { Divider, Icon, OverlayBadge, Spinner, UiPopover, UiTooltip } from "@logistics/shared/ui";
import { NotificationService, ToastService } from "@/core/services";

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
export class NotificationBell implements OnInit, OnDestroy {
  private readonly notificationService = inject(NotificationService);
  private readonly toastService = inject(ToastService);

  protected readonly popover = viewChild<UiPopover>("popover");

  /** Whether the sidebar is expanded (shows label) or collapsed (icon only) */
  public readonly expanded = input(true);

  protected readonly notifications = signal<NotificationDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly selectedNotification = signal<NotificationDto | null>(null);

  protected readonly unreadCount = computed(
    () => this.notifications().filter((n) => !n.isRead).length,
  );

  protected readonly displayedNotifications = computed(() => this.notifications().slice(0, 8));

  ngOnInit(): void {
    this.fetchNotifications();
    this.setupRealTimeNotifications();
  }

  ngOnDestroy(): void {
    this.notificationService.disconnect();
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
    const unreadNotifications = this.notifications().filter((n) => !n.isRead);
    for (const notification of unreadNotifications) {
      this.markAsRead(notification);
    }
  }

  private setupRealTimeNotifications(): void {
    this.notificationService.connect();
    this.notificationService.onReceiveNotification = (notification) => {
      this.toastService.showSuccess(notification.message ?? "", notification.title ?? undefined);
      this.notifications.update((current) => [notification, ...current]);
    };
  }

  private async fetchNotifications(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.notificationService.getPastTwoWeeksNotifications();
    if (result) {
      this.notifications.set(result);
    }

    this.isLoading.set(false);
  }

  private markAsRead(notification: NotificationDto): void {
    notification.isRead = true;
    this.notifications.update((current) => [...current]);
    if (notification.id) {
      this.notificationService.markAsRead(notification.id);
    }
  }
}
