import { computed, inject, Injectable, signal } from "@angular/core";
import { ToastService } from "@logistics/shared";
import {
  Api,
  getNotifications,
  updateNotification,
  type NotificationDto,
} from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

const DefaultWindowDays = 30;

@Injectable({ providedIn: "root" })
export class NotificationService extends BaseHubConnection {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly notificationsState = signal<NotificationDto[]>([]);

  /**
   * The one notification list, shared by the bell and the notifications page. They used to hold
   * separate copies fed by separate subscriptions, so marking everything read on the page left the
   * bell's badge showing the old count until a reload.
   */
  readonly notifications = this.notificationsState.asReadonly();

  readonly unreadCount = computed(() => this.notificationsState().filter((n) => !n.isRead).length);

  /** Every notification pushed over the hub; already toasted and prepended to {@link notifications}. */
  readonly notificationReceived$ = this.event<NotificationDto>("ReceiveNotification");

  constructor() {
    super("notification");
    this.notificationReceived$.subscribe((notification) => {
      this.toastService.showSuccess(notification.message ?? "", notification.title ?? undefined);
      this.notificationsState.update((current) => [notification, ...current]);
    });
  }

  /** Replaces the shared list with the last `days` days, newest first. */
  async load(days = DefaultWindowDays): Promise<void> {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - days);

    const result = await this.api.invoke(getNotifications, {
      StartDate: startDate.toISOString(),
      EndDate: endDate.toISOString(),
    });
    this.notificationsState.set(result ?? []);
  }

  /** Marks notifications read on the server and in the shared list. */
  async markAsRead(...notificationIds: string[]): Promise<void> {
    if (notificationIds.length === 0) {
      return;
    }

    await Promise.all(
      notificationIds.map((id) =>
        this.api.invoke(updateNotification, { id, body: { isRead: true } }),
      ),
    );

    const marked = new Set(notificationIds);
    this.notificationsState.update((list) =>
      list.map((n) => (n.id && marked.has(n.id) ? { ...n, isRead: true } : n)),
    );
  }
}
