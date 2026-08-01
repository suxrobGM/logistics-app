import { inject, Injectable } from "@angular/core";
import { ToastService } from "@logistics/shared";
import {
  Api,
  getNotifications,
  updateNotification,
  type NotificationDto,
} from "@logistics/shared/api";
import { PredefinedDateRanges } from "@/shared/utils";
import { BaseHubConnection } from "./base-hub-connection";

@Injectable({ providedIn: "root" })
export class NotificationService extends BaseHubConnection {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  /**
   * Every notification pushed over the hub. The service already showed the toast (exactly once,
   * however many consumers are mounted) - subscribers only update their own lists.
   */
  readonly notificationReceived$ = this.event<NotificationDto>("ReceiveNotification");

  constructor() {
    super("notification");
    this.notificationReceived$.subscribe((notification) => {
      this.toastService.showSuccess(notification.message ?? "", notification.title ?? undefined);
    });
  }

  async getPastTwoWeeksNotifications(): Promise<NotificationDto[]> {
    const pastTwoWeeksDateRange = PredefinedDateRanges.getPastTwoWeeks();
    return this.api.invoke(getNotifications, {
      StartDate: pastTwoWeeksDateRange.startDate.toISOString(),
      EndDate: pastTwoWeeksDateRange.endDate.toISOString(),
    });
  }

  async markAsRead(notificationId: string): Promise<void> {
    return this.api.invoke(updateNotification, {
      id: notificationId,
      body: { isRead: true },
    });
  }
}
