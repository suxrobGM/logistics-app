import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {
  CompanyStatsDto,
  CreateEmployeeCommand,
  DailyGrossesDto,
  EmployeeDto,
  MonthlyGrossesDto,
  NotificationDto,
  PagedIntervalQuery,
  PagedResult,
  RemoveEmployeeRoleCommand,
  Result,
  RoleDto,
  SearchableQuery,
  TruckStatsDto,
  UpdateEmployeeCommand,
  UpdateNotificationCommand,
} from "./models";
import {
  CustomerApiService,
  DocumentApiService,
  InvoiceApiService,
  LoadApiService,
  PaymentApiService,
  SubscriptionApiService,
  TenantApiService,
  TripApiService,
  TruckApiService,
  UserApiService,
} from "./services";
import { ReportApiService } from "./services/report-api.service";

/**
 * Facade service that provides access to various API services.
 * This service aggregates multiple API services for easier access throughout the application.
 */
@Injectable()
export class ApiService extends ApiBase {
  public readonly paymentApi = new PaymentApiService();
  public readonly tenantApi = new TenantApiService();
  public readonly userApi = new UserApiService();
  public readonly subscriptionApi = new SubscriptionApiService();
  public readonly invoiceApi = new InvoiceApiService();
  public readonly tripApi = new TripApiService();
  public readonly loadApi = new LoadApiService();
  public readonly reportApi = new ReportApiService();
  public readonly truckApi = new TruckApiService();
  public readonly customerApi = new CustomerApiService();
  public readonly documentApi = new DocumentApiService();

  // #region Employee API

  getEmployee(userId: string): Observable<Result<EmployeeDto>> {
    const url = `/employees/${userId}`;
    return this.get(url);
  }

  getEmployees(query?: SearchableQuery): Observable<PagedResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getDrivers(query?: SearchableQuery): Observable<PagedResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}&role=tenant.driver`;
    return this.get(url);
  }

  createEmployee(command: CreateEmployeeCommand): Observable<Result> {
    const url = `/employees`;
    return this.post(url, command);
  }

  removeRoleFromEmployee(command: RemoveEmployeeRoleCommand): Observable<Result> {
    const url = `/employees/${command.userId}/remove-role`;
    return this.post(url, command);
  }

  updateEmployee(command: UpdateEmployeeCommand): Observable<Result> {
    const url = `/employees/${command.userId}`;
    return this.put(url, command);
  }

  deleteEmployee(employeeId: string): Observable<Result> {
    const url = `/employees/${employeeId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Tenant Role API

  getRoles(query?: SearchableQuery): Observable<PagedResult<RoleDto>> {
    const url = `/roles/tenant?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  // #endregion

  // #region Stats API

  getCompanyStats(): Observable<Result<CompanyStatsDto>> {
    const url = `/stats/company`;
    return this.get(url);
  }

  getDailyGrosses(
    startDate: Date,
    endDate?: Date,
    truckId?: string
  ): Observable<Result<DailyGrossesDto>> {
    let url = `/stats/daily-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get(url);
  }

  getMonthlyGrosses(
    startDate: Date,
    endDate?: Date,
    truckId?: string
  ): Observable<Result<MonthlyGrossesDto>> {
    let url = `/stats/monthly-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get<Result<MonthlyGrossesDto>>(url);
  }

  getTrucksStats(query: PagedIntervalQuery): Observable<PagedResult<TruckStatsDto>> {
    const url = `/stats/trucks?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  // #endregion

  // #region Notifications API

  getNotifications(startDate: Date, endDate: Date): Observable<Result<NotificationDto[]>> {
    const url = `/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;
    return this.get(url);
  }

  updateNotification(commad: UpdateNotificationCommand): Observable<Result> {
    const url = `/notifications/${commad.id}`;
    return this.put(url, commad);
  }

  // #endregion
}
