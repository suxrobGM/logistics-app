import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import { NotificationDto, Result, UpdateNotificationCommand } from "../models";

export class NotificationApiService extends ApiBase {
  getNotifications(startDate: Date, endDate: Date): Observable<Result<NotificationDto[]>> {
    const url = `/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;
    return this.get(url);
  }

  updateNotification(commad: UpdateNotificationCommand): Observable<Result> {
    const url = `/notifications/${commad.id}`;
    return this.put(url, commad);
  }
}
