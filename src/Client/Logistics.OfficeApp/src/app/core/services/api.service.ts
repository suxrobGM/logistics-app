import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MessageService} from 'primeng/api';
import {catchError, Observable, of} from 'rxjs';
import {TenantService} from '@core/services';
import {AppConfig} from '@configs';
import {
  ResponseResult,
  Employee,
  PagedResponseResult,
  Tenant,
  Truck,
  User,
  Load,
  Role,
  DailyGrosses,
  MonthlyGrosses,
  CompanyStats,
  UpdateEmployee,
  CreateEmployee,
  RemoveEmployeeRole,
  TruckDriver,
  UpdateTruck,
  CreateTruck,
  UpdateLoad,
  CreateLoad,
  TruckStats,
  Notification,
  UpdateNotification,
  SearchableQuery,
  Customer,
  UpdateCustomer,
  CreateCustomer,
} from '../models';


@Injectable()
export class ApiService {
  private host: string;
  private headers: Record<string, string>;

  constructor(
    private httpClient: HttpClient,
    private tenantService: TenantService,
    private messageService: MessageService)
  {
    this.host = AppConfig.apiHost;
    this.headers = {'content-type': 'application/json'};
  }

  // #region Tenant API

  getTenant(): Observable<ResponseResult<Tenant>> {
    const tenantId = this.tenantService.getTenantName();
    const url = `/tenants/${tenantId}`;
    return this.get<ResponseResult<Tenant>>(url);
  }

  // #endregion


  // #region User API

  getUsers(query?: SearchableQuery): Observable<PagedResponseResult<User>> {
    const url = `/users?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<User>>(url);
  }

  // #endregion


  // #region Load API

  getLoad(id: string): Observable<ResponseResult<Load>> {
    const url = `/loads/${id}`;
    return this.get<ResponseResult<Load>>(url);
  }

  getLoads(query?: SearchableQuery, onlyActiveLoads = false): Observable<PagedResponseResult<Load>> {
    let url = `/loads?${this.stringfyQuery(query)}`;

    if (onlyActiveLoads) {
      url += '&onlyActiveLoads=true';
    }
    return this.get<PagedResponseResult<Load>>(url);
  }

  createLoad(command: CreateLoad): Observable<ResponseResult> {
    const url = `/loads`;
    return this.post<ResponseResult, CreateLoad>(url, command);
  }

  updateLoad(command: UpdateLoad): Observable<ResponseResult> {
    const url = `/loads/${command.id}`;
    return this.put<ResponseResult, UpdateLoad>(url, command);
  }

  deleteLoad(loadId: string): Observable<ResponseResult> {
    const url = `/loads/${loadId}`;
    return this.delete<ResponseResult>(url);
  }

  // #endregion


  // #region Truck API

  getTruck(truckId: string): Observable<ResponseResult<Truck>> {
    const url = `/trucks/${truckId}`;
    return this.get<ResponseResult<Truck>>(url);
  }

  getTrucks(query?: SearchableQuery): Observable<PagedResponseResult<Truck>> {
    const url = `/trucks?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<Truck>>(url);
  }

  getTruckDrivers(query?: SearchableQuery): Observable<PagedResponseResult<TruckDriver>> {
    const url = `/trucks/drivers?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<TruckDriver>>(url);
  }

  createTruck(command: CreateTruck): Observable<ResponseResult> {
    const url = `/trucks`;
    return this.post<ResponseResult, CreateTruck>(url, command);
  }

  updateTruck(command: UpdateTruck): Observable<ResponseResult> {
    const url = `/trucks/${command.id}`;
    return this.put<ResponseResult, UpdateTruck>(url, command);
  }

  deleteTruck(truckId: string): Observable<ResponseResult> {
    const url = `/trucks/${truckId}`;

    return this.delete<ResponseResult>(url);
  }

  // #endregion


  // #region Employee API

  getEmployee(userId: string): Observable<ResponseResult<Employee>> {
    const url = `/employees/${userId}`;
    return this.get<ResponseResult<Employee>>(url);
  }

  getEmployees(query?: SearchableQuery): Observable<PagedResponseResult<Employee>> {
    const url = `/employees?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<Employee>>(url);
  }

  getDrivers(query?: SearchableQuery): Observable<PagedResponseResult<Employee>> {
    const url = `/employees?${this.stringfyQuery(query)}&role=tenant.driver`;
    return this.get<PagedResponseResult<Employee>>(url);
  }

  createEmployee(command: CreateEmployee): Observable<ResponseResult> {
    const url = `/employees`;
    return this.post<ResponseResult, CreateEmployee>(url, command);
  }

  removeRoleFromEmployee(command: RemoveEmployeeRole): Observable<ResponseResult> {
    const url = `/employees/${command.userId}/remove-role`;
    return this.post<ResponseResult, RemoveEmployeeRole>(url, command);
  }

  updateEmployee(command: UpdateEmployee): Observable<ResponseResult> {
    const url = `/employees/${command.userId}`;
    return this.put<ResponseResult, UpdateEmployee>(url, command);
  }

  deleteEmployee(employeeId: string): Observable<ResponseResult> {
    const url = `/employees/${employeeId}`;
    return this.delete<ResponseResult>(url);
  }

  // #endregion


  // #region Tenant Role API

  getRoles(query?: SearchableQuery): Observable<PagedResponseResult<Role>> {
    const url = `/tenant-roles?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<Role>>(url);
  }

  // #endregion


  // #region Stats API

  getCompanyStats(): Observable<ResponseResult<CompanyStats>> {
    const url = `/stats/company`;
    return this.get<ResponseResult<CompanyStats>>(url);
  }

  getDailyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<DailyGrosses>> {
    let url = `/stats/daily-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get<ResponseResult<DailyGrosses>>(url);
  }

  getMonthlyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<MonthlyGrosses>> {
    let url = `/stats/monthly-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get<ResponseResult<MonthlyGrosses>>(url);
  }

  getTrucksStats(startDate: Date, endDate: Date, orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<TruckStats>> {
    const url = `/stats/trucks?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.get<PagedResponseResult<TruckStats>>(url);
  }

  // #endregion


  // #region Notifications API

  getNotifications(startDate: Date, endDate: Date): Observable<ResponseResult<Notification[]>> {
    const url = `/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;
    return this.get<ResponseResult<Notification[]>>(url);
  }

  updateNotification(commad: UpdateNotification): Observable<ResponseResult> {
    const url = `/notifications/${commad.id}`;
    return this.put<ResponseResult, UpdateNotification>(url, commad);
  }

  // #endregion


  // #region Customers API

  getCustomer(id: string): Observable<ResponseResult<Customer>> {
    const url = `/customers/${id}`;
    return this.get<ResponseResult<Customer>>(url);
  }

  getCustomers(query?: SearchableQuery): Observable<PagedResponseResult<Customer>> {
    const url = `/customers?${this.stringfyQuery(query)}`;
    return this.get<PagedResponseResult<Customer>>(url);
  }

  createCustomer(command: CreateCustomer): Observable<ResponseResult> {
    const url = `/customers`;
    return this.post<ResponseResult, CreateCustomer>(url, command);
  }

  updateCustomer(command: UpdateCustomer): Observable<ResponseResult> {
    const url = `/customers/${command.id}`;
    return this.put(url, command);
  }

  deleteCustomer(customerId: string): Observable<ResponseResult> {
    const url = `/customers/${customerId}`;
    return this.delete(url);
  }

  // #endregion


  parseSortProperty(sortField?: string | null, sortOrder?: number | null) {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = '';
    }

    return sortOrder <= -1 ? `-${sortField}` : sortField;
  }

  private get<TResponse>(endpoint: string): Observable<TResponse> {
    return this.httpClient
      .get<TResponse>(this.host + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  private post<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.httpClient
      .post<TResponse>(this.host + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  private put<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.httpClient
      .put<TResponse>(this.host + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  private delete<TResponse>(endpoint: string): Observable<TResponse> {
    return this.httpClient
      .delete<TResponse>(this.host + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  private stringfyQuery(query?: SearchableQuery): string {
    const search = query?.search ?? '';
    const orderBy = query?.orderBy ?? '';
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    return `search=${search}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
  }

  private handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: errorMessage});
    // this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
