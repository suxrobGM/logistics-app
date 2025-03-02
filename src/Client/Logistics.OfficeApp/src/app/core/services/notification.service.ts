import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {NotificationDto, Result} from "@/core/models";
import {PredefinedDateRanges} from "@/core/utils";
import {TenantService} from "./tenant.service";
import {BaseHubConnection} from "./base-hub-connection";
import {ApiService} from "./api.service";

@Injectable({providedIn: "root"})
export class NotificationService extends BaseHubConnection {
  constructor(
    private readonly apiService: ApiService,
    tenantService: TenantService
  ) {
    super("notification", tenantService);
  }

  set onReceiveNotification(callback: OnReceiveNotifictionFn) {
    this.hubConnection.on("ReceiveNotification", callback);
  }

  getPastTwoWeeksNotifications(): Observable<Result<NotificationDto[]>> {
    const pastTwoWeeksDateRange = PredefinedDateRanges.getPastTwoWeeks();
    return this.apiService.getNotifications(
      pastTwoWeeksDateRange.startDate,
      pastTwoWeeksDateRange.endDate
    );
  }

  markAsRead(notificationId: string): Observable<Result> {
    return this.apiService.updateNotification({
      id: notificationId,
      isRead: true,
    });
  }
}

type OnReceiveNotifictionFn = (notification: NotificationDto) => void;
