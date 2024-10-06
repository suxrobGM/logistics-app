import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {NotificationDto, Result} from '@/core/models';
import {PredefinedDateRanges} from '@/core/helpers';
import {TenantService} from './tenant.service';
import {BaseHubConnection} from './baseHubConnection';
import {ApiService} from './api.service';


@Injectable()
export class NotificationService extends BaseHubConnection {
  constructor(
    private apiService: ApiService,
    tenantService: TenantService)
  {
    super('notification', tenantService);
  }

  set onReceiveNotification(callback: OnReceiveNotifictionCallback) {
    this.hubConnection.on('ReceiveNotification', callback);
  }

  getPastTwoWeeksNotifications(): Observable<Result<NotificationDto[]>> {
    const pastTwoWeeksDateRange = PredefinedDateRanges.getPastTwoWeeks();
    return this.apiService.getNotifications(pastTwoWeeksDateRange.startDate, pastTwoWeeksDateRange.endDate);
  }

  markAsRead(notificationId: string): Observable<Result> {
    return this.apiService.updateNotification({
      id: notificationId,
      isRead: true,
    });
  }
}

type OnReceiveNotifictionCallback = (notification: NotificationDto) => void;
