import {HttpClient} from "@angular/common/http";
import {Injectable, inject} from "@angular/core";
import {Observable} from "rxjs";
import {environment} from "@/env";
import {ApiBase} from "./api-base";
import {InvoiceApi} from "./invoice.api";
import {
  CompanyStatsDto,
  CreateCustomerCommand,
  CreateEmployeeCommand,
  CreateLoadCommand,
  CreateTruckCommand,
  CustomerDto,
  DailyGrossesDto,
  EmployeeDto,
  LoadDto,
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
  UpdateLoadCommand,
  UpdateNotificationCommand,
  UpdateTruckCommand,
} from "./models";
import {PaymentApi} from "./payment.api";
import {SubscriptionApi} from "./subscription.api";
import {TenantApi} from "./tenant.api";
import {UserApi} from "./user.api";

@Injectable({providedIn: "root"})
export class ApiService extends ApiBase {
  constructor() {
    const httpClient = inject(HttpClient);

    super(environment.apiHost, httpClient);
  }

  public readonly paymentApi = new PaymentApi(this.apiUrl, this.http);
  public readonly tenantApi = new TenantApi(this.apiUrl, this.http);
  public readonly userApi = new UserApi(this.apiUrl, this.http);
  public readonly subscriptionApi = new SubscriptionApi(this.apiUrl, this.http);
  public readonly invoiceApi = new InvoiceApi(this.apiUrl, this.http);

  // #region Load API

  getLoad(id: string): Observable<Result<LoadDto>> {
    const url = `/loads/${id}`;
    return this.get(url);
  }

  getLoads(query?: SearchableQuery, onlyActiveLoads = false): Observable<PagedResult<LoadDto>> {
    let url = `/loads?${this.stringfySearchableQuery(query)}`;

    if (onlyActiveLoads) {
      url += "&onlyActiveLoads=true";
    }
    return this.get(url);
  }

  createLoad(command: CreateLoadCommand): Observable<Result> {
    const url = `/loads`;
    return this.post(url, command);
  }

  updateLoad(command: UpdateLoadCommand): Observable<Result> {
    const url = `/loads/${command.id}`;
    return this.put(url, command);
  }

  deleteLoad(loadId: string): Observable<Result> {
    const url = `/loads/${loadId}`;
    return this.delete(url);
  }

  // #endregion

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
