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
  Payment,
  CreatePayment,
  UpdatePayment,
  PagedQuery,
  PagedIntervalQuery,
  Invoice,
  CreateInvoice,
  UpdateInvoice,
  Payroll,
  UpdatePayroll,
  CreatePayroll,
  ProcessPayment,
} from '../models';


@Injectable()
export class ApiService {
  private host: string;
  private headers: Record<string, string>;

  constructor(
    private readonly httpClient: HttpClient,
    private readonly tenantService: TenantService,
    private readonly messageService: MessageService)
  {
    this.host = AppConfig.apiHost;
    this.headers = {'content-type': 'application/json'};
  }

  // #region Tenant API

  getTenant(): Observable<ResponseResult<Tenant>> {
    const tenantId = this.tenantService.getTenantName();
    const url = `/tenants/${tenantId}`;
    return this.get(url);
  }

  // #endregion


  // #region User API

  getUsers(query?: SearchableQuery): Observable<PagedResponseResult<User>> {
    const url = `/users?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  // #endregion


  // #region Load API

  getLoad(id: string): Observable<ResponseResult<Load>> {
    const url = `/loads/${id}`;
    return this.get(url);
  }

  getLoads(query?: SearchableQuery, onlyActiveLoads = false): Observable<PagedResponseResult<Load>> {
    let url = `/loads?${this.stringfySearchableQuery(query)}`;

    if (onlyActiveLoads) {
      url += '&onlyActiveLoads=true';
    }
    return this.get(url);
  }

  createLoad(command: CreateLoad): Observable<ResponseResult> {
    const url = `/loads`;
    return this.post(url, command);
  }

  updateLoad(command: UpdateLoad): Observable<ResponseResult> {
    const url = `/loads/${command.id}`;
    return this.put(url, command);
  }

  deleteLoad(loadId: string): Observable<ResponseResult> {
    const url = `/loads/${loadId}`;
    return this.delete(url);
  }

  // #endregion


  // #region Truck API

  getTruck(truckId: string): Observable<ResponseResult<Truck>> {
    const url = `/trucks/${truckId}`;
    return this.get(url);
  }

  getTrucks(query?: SearchableQuery): Observable<PagedResponseResult<Truck>> {
    const url = `/trucks?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getTruckDrivers(query?: SearchableQuery): Observable<PagedResponseResult<TruckDriver>> {
    const url = `/trucks/drivers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createTruck(command: CreateTruck): Observable<ResponseResult> {
    const url = `/trucks`;
    return this.post(url, command);
  }

  updateTruck(command: UpdateTruck): Observable<ResponseResult> {
    const url = `/trucks/${command.id}`;
    return this.put(url, command);
  }

  deleteTruck(truckId: string): Observable<ResponseResult> {
    const url = `/trucks/${truckId}`;
    return this.delete(url);
  }

  // #endregion


  // #region Employee API

  getEmployee(userId: string): Observable<ResponseResult<Employee>> {
    const url = `/employees/${userId}`;
    return this.get(url);
  }

  getEmployees(query?: SearchableQuery): Observable<PagedResponseResult<Employee>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getDrivers(query?: SearchableQuery): Observable<PagedResponseResult<Employee>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}&role=tenant.driver`;
    return this.get(url);
  }

  createEmployee(command: CreateEmployee): Observable<ResponseResult> {
    const url = `/employees`;
    return this.post(url, command);
  }

  removeRoleFromEmployee(command: RemoveEmployeeRole): Observable<ResponseResult> {
    const url = `/employees/${command.userId}/remove-role`;
    return this.post(url, command);
  }

  updateEmployee(command: UpdateEmployee): Observable<ResponseResult> {
    const url = `/employees/${command.userId}`;
    return this.put(url, command);
  }

  deleteEmployee(employeeId: string): Observable<ResponseResult> {
    const url = `/employees/${employeeId}`;
    return this.delete(url);
  }

  // #endregion


  // #region Tenant Role API

  getRoles(query?: SearchableQuery): Observable<PagedResponseResult<Role>> {
    const url = `/tenant-roles?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  // #endregion


  // #region Stats API

  getCompanyStats(): Observable<ResponseResult<CompanyStats>> {
    const url = `/stats/company`;
    return this.get(url);
  }

  getDailyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<DailyGrosses>> {
    let url = `/stats/daily-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get(url);
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

  getTrucksStats(query: PagedIntervalQuery): Observable<PagedResponseResult<TruckStats>> {
    const url = `/stats/trucks?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  // #endregion


  // #region Notifications API

  getNotifications(startDate: Date, endDate: Date): Observable<ResponseResult<Notification[]>> {
    const url = `/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;
    return this.get(url);
  }

  updateNotification(commad: UpdateNotification): Observable<ResponseResult> {
    const url = `/notifications/${commad.id}`;
    return this.put(url, commad);
  }

  // #endregion


  // #region Customers API

  getCustomer(id: string): Observable<ResponseResult<Customer>> {
    const url = `/customers/${id}`;
    return this.get(url);
  }

  getCustomers(query?: SearchableQuery): Observable<PagedResponseResult<Customer>> {
    const url = `/customers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createCustomer(command: CreateCustomer): Observable<ResponseResult> {
    const url = `/customers`;
    return this.post(url, command);
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


  // #region Payments API
  
  getPayment(id: string): Observable<ResponseResult<Payment>> {
    const url = `/payments/${id}`;
    return this.get(url);
  }

  getPayments(query?: PagedIntervalQuery): Observable<PagedResponseResult<Payment>> {
    const url = `/payments?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  processPayment(command: ProcessPayment): Observable<ResponseResult> {
    const url = `/payments/process-payment`;
    return this.post(url, command);
  }

  createPayment(command: CreatePayment): Observable<ResponseResult> {
    const url = `/payments`;
    return this.post(url, command);
  }

  updatePayment(command: UpdatePayment): Observable<ResponseResult> {
    const url = `/payments/${command.id}`;
    return this.put(url, command);
  }

  deletePayment(paymentId: string): Observable<ResponseResult> {
    const url = `/payments/${paymentId}`;
    return this.delete(url);
  }

  // #endregion


  // #region Invoices API
  
  getInvoice(id: string): Observable<ResponseResult<Invoice>> {
    const url = `/invoices/${id}`;
    return this.get(url);
  }

  getInvoices(query?: PagedIntervalQuery): Observable<PagedResponseResult<Invoice>> {
    const url = `/invoices?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  createInvoice(command: CreateInvoice): Observable<ResponseResult> {
    const url = `/invoices`;
    return this.post(url, command);
  }

  updateInvoice(command: UpdateInvoice): Observable<ResponseResult> {
    const url = `/invoices/${command.id}`;
    return this.put(url, command);
  }

  deleteInvoice(invoiceId: string): Observable<ResponseResult> {
    const url = `/invoices/${invoiceId}`;
    return this.delete(url);
  }

  // #endregion


  // #region Payrolls API
  
  getPayroll(id: string): Observable<ResponseResult<Payroll>> {
    const url = `/payrolls/${id}`;
    return this.get(url);
  }

  getPayrolls(query?: SearchableQuery): Observable<PagedResponseResult<Payroll>> {
    const url = `/payrolls?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  calculateEmployeePayroll(query: CreatePayroll): Observable<ResponseResult<Payroll>> {
    const url = `/payrolls/calculate?employeeId=${query.employeeId}&startDate=${query.startDate.toJSON()}&endDate=${query.endDate.toJSON()}`;
    return this.get(url);
  }

  createPayroll(command: CreatePayroll): Observable<ResponseResult> {
    const url = `/payrolls`;
    return this.post(url, command);
  }

  updatePayroll(command: UpdatePayroll): Observable<ResponseResult> {
    const url = `/payrolls/${command.id}`;
    return this.put(url, command);
  }

  deletePayroll(payrollId: string): Observable<ResponseResult> {
    const url = `/payrolls/${payrollId}`;
    return this.delete(url);
  }

  // #endregion



  parseSortProperty(sortField?: string | null, sortOrder?: number | null): string {
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

  private stringfySearchableQuery(query?: SearchableQuery): string {
    const search = query?.search ?? '';
    const orderBy = query?.orderBy ?? '';
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    return `search=${search}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
  }

  private stringfyPagedQuery(query?: PagedQuery): string {
    const orderBy = query?.orderBy ?? '';
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    return `orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
  }

  private stringfyPagedIntervalQuery(query?: PagedIntervalQuery): string {
    const startDate = query?.startDate.toJSON() ?? new Date().toJSON();
    const endDate = query?.endDate?.toJSON();
    const orderBy = query?.orderBy ?? '';
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    let queryStr = `startDate=${startDate}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;

    if (endDate) {
      queryStr += `&endDate=${endDate}`
    }

    return queryStr;
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: errorMessage});
    // this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
