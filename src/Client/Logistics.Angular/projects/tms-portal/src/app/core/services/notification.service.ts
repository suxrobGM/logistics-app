import { Injectable, inject } from "@angular/core";
import {
  Api,
  type NotificationDto,
  getNotifications,
  updateNotification,
} from "@logistics/shared/api";
import { PredefinedDateRanges } from "@/shared/utils";
import { BaseHubConnection } from "./base-hub-connection";

@Injectable({ providedIn: "root" })
export class NotificationService extends BaseHubConnection {
  private readonly api = inject(Api);

  constructor() {
    super("notification");
  }

  set onReceiveNotification(callback: OnReceiveNotifictionFn) {
    this.hubConnection.on("ReceiveNotification", callback);
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

type OnReceiveNotifictionFn = (notification: NotificationDto) => void;
