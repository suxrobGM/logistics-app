import { CommonModule } from "@angular/common";
import { Component, type OnDestroy, inject, model, signal } from "@angular/core";
import { TimeAgoPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { OverlayBadgeModule } from "primeng/overlaybadge";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import type { NotificationDto } from "@/core/api/models";
import { NotificationService, ToastService } from "@/core/services";

@Component({
  selector: "app-notifications-panel",
  templateUrl: "./notifications-panel.html",
  styleUrl: "./notifications-panel.css",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TimeAgoPipe,
    OverlayBadgeModule,
    ProgressSpinnerModule,
    DialogModule,
  ],
})
export class NotificationsPanelComponent implements OnDestroy {
  private readonly notificationService = inject(NotificationService);
  private readonly toastService = inject(ToastService);

  protected readonly notifications = signal<NotificationDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly showDialog = signal(false);
  protected readonly selectedNotification = signal<NotificationDto | null>(null);

  public readonly height = model("100%");

  constructor() {
    this.fetchNotifications();

    this.notificationService.connect();
    this.notificationService.onReceiveNotification = (notification) => {
      this.toastService.showSuccess(notification.message ?? "", notification.title ?? undefined);
      this.notifications.update((current) => [notification, ...current]);
    };
  }

  ngOnDestroy(): void {
    this.notificationService.disconnect();
  }

  async fetchNotifications(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.notificationService.getPastTwoWeeksNotifications();
    if (result) {
      this.notifications.set(result);
    }

    this.isLoading.set(false);
  }

  showNotification(notification: NotificationDto): void {
    this.selectedNotification.set(notification);
    this.showDialog.set(true);

    if (!notification.isRead) {
      this.markAsRead(notification);
    }
  }

  closeDialog(): void {
    this.showDialog.set(false);
  }

  markAsRead(notification: NotificationDto): void {
    // Update frontend state
    notification.isRead = true;
    if (notification.id) {
      this.notificationService.markAsRead(notification.id);
    }
  }

  getUnreadNotificationsCount(): string {
    return this.notifications()
      .filter((i) => !i.isRead)
      .length.toString();
  }
}
