import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {
  CompanyStatsDto,
  CreateCustomerCommand,
  CreateEmployeeCommand,
  CreateTruckCommand,
  CustomerDto,
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
  TruckDriverDto,
  TruckDto,
  TruckStatsDto,
  UpdateCustomerCommand,
  UpdateEmployeeCommand,
  UpdateNotificationCommand,
  UpdateTruckCommand,
} from "./models";
import {
  InvoiceApiService,
  LoadApiService,
  PaymentApiService,
  SubscriptionApiService,
  TenantApiService,
  TripApiService,
  UserApiService,
} from "./services";

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

  // #region Truck API

  getTruck(truckId: string): Observable<Result<TruckDto>> {
    const url = `/trucks/${truckId}`;
    return this.get(url);
  }

  getTrucks(query?: SearchableQuery): Observable<PagedResult<TruckDto>> {
    const url = `/trucks?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getTruckDrivers(query?: SearchableQuery): Observable<PagedResult<TruckDriverDto>> {
    const url = `/trucks/drivers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createTruck(command: CreateTruckCommand): Observable<Result> {
    const url = `/trucks`;
    return this.post(url, command);
  }

  updateTruck(command: UpdateTruckCommand): Observable<Result> {
    const url = `/trucks/${command.id}`;
    return this.put(url, command);
  }

  deleteTruck(truckId: string): Observable<Result> {
    const url = `/trucks/${truckId}`;
    return this.delete(url);
  }

  // #endregion

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

  // #region Customers API

  getCustomer(id: string): Observable<Result<CustomerDto>> {
    const url = `/customers/${id}`;
    return this.get(url);
  }

  getCustomers(query?: SearchableQuery): Observable<PagedResult<CustomerDto>> {
    const url = `/customers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createCustomer(command: CreateCustomerCommand): Observable<Result> {
    const url = `/customers`;
    return this.post(url, command);
  }

  updateCustomer(command: UpdateCustomerCommand): Observable<Result> {
    const url = `/customers/${command.id}`;
    return this.put(url, command);
  }

  deleteCustomer(customerId: string): Observable<Result> {
    const url = `/customers/${customerId}`;
    return this.delete(url);
  }

  // #endregion
}
